namespace Backups.Core.Application.UseCases;

using System;
using System.Collections.Generic;
using System.Linq;
using Backups.Core.Domain.Entities;
using Backups.Core.Domain.Services;
using Backups.Core.Ports.In;
using Backups.Core.Ports.Out;

/// <summary>
/// Caso de uso principal: Ejecutar un respaldo.
/// Responsabilidad: Orquestar el flujo completo (verificar cambios → comprimir → transmitir).
/// IMPLEMENTA la interfaz: IEjecutarRespaldoUseCase
/// </summary>
public class EjecutarRespaldoUseCase : IEjecutarRespaldoUseCase
{
    private readonly VerificadorCambios _verificador;
    private readonly GestorLog _gestorLog;

    private readonly ICompressionService _compressionService;
    private readonly ITransmissionService _transmissionService;

    public EjecutarRespaldoUseCase(
        VerificadorCambios verificador,
        GestorLog gestorLog,
        ICompressionService compressionService,
        ITransmissionService transmissionService)
    {
        _verificador = verificador;
        _gestorLog = gestorLog;
        _compressionService = compressionService;
        _transmissionService = transmissionService;
    }

    /// <summary>
    /// Método principal del caso de uso.
    /// IMPLEMENTA: IEjecutarRespaldoUseCase.Ejecutar
    /// </summary>
    public void Ejecutar(SolicitudRespaldo solicitud)
    {
        var backupId = Guid.NewGuid().ToString();

        try
        {
            var hashActual = _verificador.CalcularHashArchivo(solicitud.RutaOrigen);
            var ultimoHashConfirmado = ObtenerUltimoHashConfirmado(solicitud.RutaOrigen);

            bool cambio = _verificador.ArchivoFueCambiado(ultimoHashConfirmado, hashActual);

            if (!cambio)
            {
                var registroOmitido = new RegistroRespaldo
                {
                    BackupId = backupId,
                    Estado = "OMITIDO_SIN_CAMBIOS",
                    MensajeTexto = $"Copia cancelada: El archivo plano no presenta modificaciones desde el último respaldo. Ruta: {solicitud.RutaOrigen}",
                    FechaRegistro = DateTime.Now,
                    RutaOrigen = solicitud.RutaOrigen,
                    HashArchivo = hashActual
                };
                _gestorLog.RegistrarEvento(registroOmitido);
                Console.WriteLine($"[Grupo 1] OMITIDO: {backupId}");
                return;
            }

            var registroProcesando = new RegistroRespaldo
            {
                BackupId = backupId,
                Estado = "PROCESANDO",
                MensajeTexto = $"Iniciando respaldo de {solicitud.NombreCopia}. Archivo: {solicitud.RutaOrigen}",
                FechaRegistro = DateTime.Now,
                RutaOrigen = solicitud.RutaOrigen,
                HashArchivo = hashActual
            };
            _gestorLog.RegistrarEvento(registroProcesando);
            Console.WriteLine($"[Grupo 1] PROCESANDO: {backupId}");

            List<string> rutasComprimidas = _compressionService.ComprimirYSegmentar(
                solicitud.RutaOrigen,
                solicitud.AlgoritmoCompresion,
                solicitud.LimiteVolumenMb
            );

            if (rutasComprimidas == null || rutasComprimidas.Count == 0)
            {
                throw new Exception("Grupo 2 retornó lista vacía de archivos comprimidos");
            }

            Console.WriteLine($"[Grupo 1] Compresión OK: {rutasComprimidas.Count} volumen(es)");

            bool enviado = _transmissionService.EnviarArchivos(
                rutasComprimidas,
                solicitud.IdDestinoConfig
            );

            var estadoFinal = enviado ? "COMPLETADO" : "FALLIDO";
            var mensajeFinal = enviado
                ? $"Respaldo completado exitosamente. ID: {backupId}, Volúmenes: {rutasComprimidas.Count}"
                : $"Error en transmisión. ID: {backupId}";

            var registroFinal = new RegistroRespaldo
            {
                BackupId = backupId,
                Estado = estadoFinal,
                MensajeTexto = mensajeFinal,
                FechaRegistro = DateTime.Now,
                RutaOrigen = solicitud.RutaOrigen,
                HashArchivo = enviado ? hashActual : string.Empty
            };
            _gestorLog.RegistrarEvento(registroFinal);
            Console.WriteLine($"[Grupo 1] {estadoFinal}: {backupId}");
        }
        catch (Exception ex)
        {
            var registroError = new RegistroRespaldo
            {
                BackupId = backupId,
                Estado = "FALLIDO",
                MensajeTexto = $"Error en Grupo 1: {ex.GetType().Name} - {ex.Message}",
                FechaRegistro = DateTime.Now,
                RutaOrigen = solicitud.RutaOrigen
            };
            _gestorLog.RegistrarEvento(registroError);
            Console.WriteLine($"[Grupo 1] ERROR: {backupId} - {ex.Message}");
        }
    }

    /// <summary>
    /// Busca en el historial el último hash confirmado para esta ruta (respaldo
    /// COMPLETADO u OMITIDO_SIN_CAMBIOS). Un intento FALLIDO no cuenta como confirmado,
    /// para que el siguiente respaldo se reintente aunque el archivo no haya cambiado.
    /// </summary>
    private string ObtenerUltimoHashConfirmado(string rutaOrigen)
    {
        return _gestorLog.ObtenerHistorial()
            .Where(r => string.Equals(r.RutaOrigen, rutaOrigen, StringComparison.OrdinalIgnoreCase)
                        && !string.IsNullOrEmpty(r.HashArchivo)
                        && (r.Estado == "COMPLETADO" || r.Estado == "OMITIDO_SIN_CAMBIOS"))
            .OrderByDescending(r => r.FechaRegistro)
            .Select(r => r.HashArchivo)
            .FirstOrDefault() ?? string.Empty;
    }
}
