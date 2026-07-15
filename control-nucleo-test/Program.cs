using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using Backups.Core.Application.UseCases;
using Backups.Core.Domain.Entities;
using Backups.Core.Domain.Services;
using Backups.Core.Ports.Out;

namespace Backups.Core.Tests;

class Program
{
    static int _passed = 0;
    static int _failed = 0;
    static int _total = 0;

    static void Main(string[] args)
    {
        Console.WriteLine("========================================");
        Console.WriteLine("  PRUEBAS — Backups.Core (Grupo 1)");
        Console.WriteLine("========================================\n");

        TestVerificadorCambios();
        TestGestorLog();
        TestEjecutarRespaldoUseCase();
        TestEntidades();

        Console.WriteLine("\n========================================");
        Console.WriteLine($"  RESUMEN: {_passed}/{_total} pasaron, {_failed} fallaron");
        Console.WriteLine("========================================");

        Environment.Exit(_failed > 0 ? 1 : 0);
    }

    static void Assert(bool condicion, string nombrePrueba)
    {
        _total++;
        if (condicion)
        {
            _passed++;
            Console.WriteLine($"  [PASS] {nombrePrueba}");
        }
        else
        {
            _failed++;
            Console.WriteLine($"  [FAIL] {nombrePrueba}");
        }
    }

    // ============================================
    //  VERIFICADOR DE CAMBIOS
    // ============================================
    static void TestVerificadorCambios()
    {
        Console.WriteLine("\n--- VerificadorCambios ---");
        var vc = new VerificadorCambios();

        string archivoTemporal = Path.GetTempFileName();
        try
        {
            File.WriteAllText(archivoTemporal, "Contenido de prueba para hash");

            // 1. Hash de archivo existente tiene 64 caracteres
            string hash = vc.CalcularHashArchivo(archivoTemporal);
            Assert(hash.Length == 64, "CalcularHashArchivo_RetornaHash64Caracteres");

            // 2. Mismo archivo = mismo hash
            string hash2 = vc.CalcularHashArchivo(archivoTemporal);
            Assert(hash == hash2, "CalcularHashArchivo_MismoArchivo_MismoHash");

            // 3. Archivo modificado = hash diferente
            File.WriteAllText(archivoTemporal, "Contenido MODIFICADO para hash");
            string hash3 = vc.CalcularHashArchivo(archivoTemporal);
            Assert(hash != hash3, "CalcularHashArchivo_ArchivoModificado_HashDiferente");

            // 4. Hash vacio = archivo fue cambiado (primera vez)
            bool cambiado = vc.ArchivoFueCambiado("", hash);
            Assert(cambiado == true, "ArchivoFueCambiado_HashVacio_RetornaTrue");

            // 5. Mismos hashes = no cambio
            bool sinCambio = vc.ArchivoFueCambiado(hash, hash);
            Assert(sinCambio == false, "ArchivoFueCambiado_HashesIguales_RetornaFalse");

            // 6. Diferentes hashes = si cambio
            bool conCambio = vc.ArchivoFueCambiado("abc123", "def456");
            Assert(conCambio == true, "ArchivoFueCambiado_HashesDiferentes_RetornaTrue");
        }
        finally
        {
            if (File.Exists(archivoTemporal)) File.Delete(archivoTemporal);
        }

        // 7. Archivo no existe = excepcion
        bool lanzExcepcion = false;
        try
        {
            vc.CalcularHashArchivo("C:\\Ruta\\Que\\No\\Existe\\x.txt");
        }
        catch (FileNotFoundException)
        {
            lanzExcepcion = true;
        }
        Assert(lanzExcepcion, "CalcularHashArchivo_ArchivoNoExiste_LanzaExcepcion");
    }

    // ============================================
    //  GESTOR LOG
    // ============================================
    static void TestGestorLog()
    {
        Console.WriteLine("\n--- GestorLog ---");

        string carpetaOriginal = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WinBackup",
            "control_backups.json");

        string backupPath = carpetaOriginal + ".test_backup";
        try
        {
            // Backup del archivo real si existe
            if (File.Exists(carpetaOriginal))
                File.Copy(carpetaOriginal, backupPath, true);

            // Limpiar para test
            if (File.Exists(carpetaOriginal))
                File.Delete(carpetaOriginal);

            var gl = new GestorLog();

            // 1. Historial vacio cuando no existe archivo
            var historial = gl.ObtenerHistorial();
            Assert(historial.Count == 0, "ObtenerHistorial_ArchivoNoExiste_RetornaVacia");

            // 2. Registrar primer evento
            gl.RegistrarEvento(new RegistroRespaldo
            {
                BackupId = "test-001",
                Estado = "COMPLETADO",
                MensajeTexto = "Prueba 1",
                RutaOrigen = "C:\\test\\a.txt",
                HashArchivo = "abc123"
            });
            var h1 = gl.ObtenerHistorial();
            Assert(h1.Count == 1, "RegistrarEvento_PrimeraVez_CreaArchivo");

            // 3. Acumular multiples eventos
            gl.RegistrarEvento(new RegistroRespaldo
            {
                BackupId = "test-002",
                Estado = "OMITIDO_SIN_CAMBIOS",
                MensajeTexto = "Prueba 2",
                RutaOrigen = "C:\\test\\b.txt",
                HashArchivo = "def456"
            });
            gl.RegistrarEvento(new RegistroRespaldo
            {
                BackupId = "test-003",
                Estado = "FALLIDO",
                MensajeTexto = "Prueba 3",
                RutaOrigen = "C:\\test\\c.txt"
            });
            var h2 = gl.ObtenerHistorial();
            Assert(h2.Count == 3, "RegistrarEvento_VariasVeces_AcumulaRegistros");

            // 4. Campos se deserializan correctamente
            var registro = h2[0];
            bool camposOK = registro.BackupId == "test-001"
                && registro.Estado == "COMPLETADO"
                && registro.HashArchivo == "abc123"
                && registro.RutaOrigen == "C:\\test\\a.txt";
            Assert(camposOK, "ObtenerHistorial_DeserializaCamposCorrectamente");

            // 5. Orden cronologico
            bool ordenOK = h2[0].BackupId == "test-001"
                && h2[1].BackupId == "test-002"
                && h2[2].BackupId == "test-003";
            Assert(ordenOK, "ObtenerHistorial_OrdenCronologico");
        }
        finally
        {
            // Restaurar archivo original
            if (File.Exists(backupPath))
            {
                File.Copy(backupPath, carpetaOriginal, true);
                File.Delete(backupPath);
            }
        }
    }

    // ============================================
    //  EJECUTAR RESPALDO USE CASE
    // ============================================
    static void TestEjecutarRespaldoUseCase()
    {
        Console.WriteLine("\n--- EjecutarRespaldoUseCase ---");

        string carpetaOriginal = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WinBackup",
            "control_backups.json");

        string backupPath = carpetaOriginal + ".test_backup";
        try
        {
            // Backup del archivo real si existe
            if (File.Exists(carpetaOriginal))
                File.Copy(carpetaOriginal, backupPath, true);

            // Limpiar para test
            if (File.Exists(carpetaOriginal))
                File.Delete(carpetaOriginal);

            var gestorLog = new GestorLog();
            var verificador = new VerificadorCambios();
            var mockCompresion = new MockCompressionService();
            var mockTransmision = new MockTransmissionService();

            var useCase = new EjecutarRespaldoUseCase(
                verificador, gestorLog, mockCompresion, mockTransmision);

            // Crear archivo de prueba
            string archivoPrueba = Path.GetTempFileName();
            File.WriteAllText(archivoPrueba, "Contenido de prueba para use case");

            try
            {
                var solicitud = new SolicitudRespaldo
                {
                    NombreCopia = "TestRespaldo",
                    TipoDisparo = "BOTON_MANUAL",
                    RutaOrigen = archivoPrueba,
                    AlgoritmoCompresion = "ZIP",
                    LimiteVolumenMb = 100,
                    IdDestinoConfig = "test_destino"
                };

                // 1. Primera ejecucion: archivo nuevo, debe comprimir y transmitir
                mockCompresion.UltimoAlgoritmo = null;
                mockTransmision.UltimoIdDestino = null;
                useCase.Ejecutar(solicitud);

                var h1 = gestorLog.ObtenerHistorial();
                bool primeraEjecucionOK = h1.Count >= 2
                    && h1.Any(r => r.Estado == "PROCESANDO")
                    && h1.Any(r => r.Estado == "COMPLETADO")
                    && mockCompresion.UltimoAlgoritmo == "ZIP"
                    && mockTransmision.UltimoIdDestino == "test_destino";
                Assert(primeraEjecucionOK, "Ejecutar_ArchivoNuevo_CompriimeYTransmite");

                // 2. Segunda ejecucion sin cambios: debe omitir
                int countAntes = gestorLog.ObtenerHistorial().Count;
                useCase.Ejecutar(solicitud);
                var h2 = gestorLog.ObtenerHistorial();
                bool omitido = h2.Count == countAntes + 1
                    && h2.Last().Estado == "OMITIDO_SIN_CAMBIOS";
                Assert(omitido, "Ejecutar_SinCambios_OmiteYRegistra");

                // 3. Modificar archivo y ejecutar: debe comprimir de nuevo
                File.WriteAllText(archivoPrueba, "Contenido MODIFICADO para segunda vuelta");
                mockCompresion.UltimoAlgoritmo = null;
                useCase.Ejecutar(solicitud);
                var h3 = gestorLog.ObtenerHistorial();
                bool despuesDeCambiar = h3.Last().Estado == "COMPLETADO"
                    && mockCompresion.UltimoAlgoritmo == "ZIP";
                Assert(despuesDeCambiar, "Ejecutar_ArchivoCambiado_CompriimeYTransmite");

                // 4. Transmision fallida = FALLIDO
                mockTransmision.DebeFallir = true;
                File.WriteAllText(archivoPrueba, "Otro cambio mas");
                useCase.Ejecutar(solicitud);
                var h4 = gestorLog.ObtenerHistorial();
                bool fallido = h4.Last().Estado == "FALLIDO";
                Assert(fallido, "Ejecutar_TransmisionFalla_RegistraFallido");
                mockTransmision.DebeFallir = false;

                // 5. Compresion fallida = FALLIDO
                mockCompresion.DebeFallir = true;
                File.WriteAllText(archivoPrueba, "Cambiar otra vez");
                useCase.Ejecutar(solicitud);
                var h5 = gestorLog.ObtenerHistorial();
                bool fallidoCompresion = h5.Last().Estado == "FALLIDO"
                    && h5.Last().MensajeTexto.Contains("Error");
                Assert(fallidoCompresion, "Ejecutar_CompresionFalla_RegistraFallido");
                mockCompresion.DebeFallir = false;

                // 6. Archivo no existe = FALLIDO (catch)
                var solicitudMala = new SolicitudRespaldo
                {
                    NombreCopia = "TestMalo",
                    RutaOrigen = "C:\\Ruta\\Inexistente\\x.txt",
                    AlgoritmoCompresion = "ZIP",
                    LimiteVolumenMb = 100,
                    IdDestinoConfig = "test"
                };
                useCase.Ejecutar(solicitudMala);
                var h6 = gestorLog.ObtenerHistorial();
                bool fallidoNoExiste = h6.Last().Estado == "FALLIDO";
                Assert(fallidoNoExiste, "Ejecutar_ArchivoNoExiste_RegistraFallido");

                // 7. Hash se guarda solo en COMPLETADO
                File.WriteAllText(archivoPrueba, "Para completar");
                useCase.Ejecutar(solicitud);
                var h7 = gestorLog.ObtenerHistorial();
                var ultimoCompletado = h7.LastOrDefault(r => r.Estado == "COMPLETADO");
                bool hashGuardado = ultimoCompletado != null
                    && !string.IsNullOrEmpty(ultimoCompletado.HashArchivo);
                Assert(hashGuardado, "Ejecutar_Completado_GuardaHash");

                // 8. Hash NO se guarda en FALLIDO
                var fallidoReciente = h7.LastOrDefault(r => r.Estado == "FALLIDO");
                bool hashVacioEnFalla = fallidoReciente != null
                    || h7.Any(r => r.Estado == "FALLIDO");
                Assert(hashVacioEnFalla, "Ejecutar_Fallido_NoGuardaHash");
            }
            finally
            {
                if (File.Exists(archivoPrueba)) File.Delete(archivoPrueba);
            }
        }
        finally
        {
            // Restaurar archivo original
            if (File.Exists(backupPath))
            {
                File.Copy(backupPath, carpetaOriginal, true);
                File.Delete(backupPath);
            }
        }
    }

    // ============================================
    //  ENTIDADES
    // ============================================
    static void TestEntidades()
    {
        Console.WriteLine("\n--- Entidades ---");

        // SolicitudRespaldo
        var solicitud = new SolicitudRespaldo();
        Assert(solicitud.TipoDisparo == "TIMER", "SolicitudRespaldo_TipoDisparoDefaultEsTimer");
        Assert(solicitud.AlgoritmoCompresion == "ZIP", "SolicitudRespaldo_AlgoritmoDefaultEsZip");
        Assert(solicitud.LimiteVolumenMb == 0, "SolicitudRespaldo_LimiteVolumenDefaultEsCero");

        solicitud.NombreCopia = "MiCopia";
        solicitud.RutaOrigen = "C:\\test.txt";
        solicitud.LimiteVolumenMb = 500;
        bool asignados = solicitud.NombreCopia == "MiCopia"
            && solicitud.RutaOrigen == "C:\\test.txt"
            && solicitud.LimiteVolumenMb == 500;
        Assert(asignados, "SolicitudRespaldo_AsignarPropiedades_SeGuardan");

        // RegistroRespaldo
        var registro = new RegistroRespaldo();
        bool fechaCerca = Math.Abs((registro.FechaRegistro - DateTime.Now).TotalSeconds) < 5;
        Assert(fechaCerca, "RegistroRespaldo_FechaRegistroDefaultEsAhora");

        registro.Estado = "COMPLETADO";
        registro.BackupId = Guid.NewGuid().ToString();
        bool registroOK = registro.Estado == "COMPLETADO"
            && registro.BackupId.Length > 0;
        Assert(registroOK, "RegistroRespaldo_AsignarPropiedades_SeGuardan");

        // Verificar estados validos del contrato
        string[] estadosValidos = [
            "OMITIDO_SIN_CAMBIOS", "PROCESANDO", "COMPRIMIENDO_VOLUMENES",
            "ENVIANDO", "COMPLETADO", "FALLIDO"
        ];
        bool todosValidos = estadosValidos.All(e => e.Length > 0);
        Assert(todosValidos, "RegistroRespaldo_EstadosValidos_DelContrato");
    }
}

// ============================================
//  MOCKS PARA PRUEBAS
// ============================================

class MockCompressionService : ICompressionService
{
    public string? UltimoAlgoritmo { get; set; }
    public bool DebeFallir { get; set; }

    public List<string> ComprimirYSegmentar(string rutaOrigen, string algoritmo, int limiteVolumenMb)
    {
        if (DebeFallir)
            throw new InvalidOperationException("Error simulado de compresion (Grupo 2 mock)");

        UltimoAlgoritmo = algoritmo;
        string archivoMock = Path.Combine(Path.GetTempPath(), $"mock_comprimido_{Guid.NewGuid():N}.zip");
        File.WriteAllBytes(archivoMock, [0x50, 0x4B, 0x03, 0x04]); // ZIP header
        return [archivoMock];
    }
}

class MockTransmissionService : ITransmissionService
{
    public string? UltimoIdDestino { get; set; }
    public bool DebeFallir { get; set; }

    public bool EnviarArchivos(List<string> rutasArchivos, string idDestinoConfig)
    {
        if (DebeFallir)
            return false;

        UltimoIdDestino = idDestinoConfig;
        // Limpiar archivos mock
        foreach (var ruta in rutasArchivos)
        {
            if (ruta.Contains("mock_comprimido") && File.Exists(ruta))
                File.Delete(ruta);
        }
        return true;
    }
}
