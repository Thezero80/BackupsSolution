using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Backups.Core.Ports.Out;
using FluentFTP;
using Renci.SshNet;

namespace Backups.Infrastructure.Network;

public class NetworkTransmissionAdapter : ITransmissionService
{
    private const string ArchivoConfiguracion = "destinos.json";

    public bool EnviarArchivos(List<string> rutasArchivos, string idDestinoConfig)
    {
        try
        {
            foreach (var ruta in rutasArchivos)
            {
                if (!File.Exists(ruta))
                    throw new FileNotFoundException($"Grupo 2 no entregó el archivo esperado: {ruta}");
            }

            var config = ObtenerConfiguracion(idDestinoConfig);

            switch (config.Protocolo.ToUpperInvariant())
            {
                case "FTP":
                    EnviarPorFtp(rutasArchivos, config);
                    break;
                case "SFTP":
                case "SSH":
                    EnviarPorSftp(rutasArchivos, config);
                    break;
                default:
                    throw new NotSupportedException($"Protocolo de transmisión no soportado: {config.Protocolo}");
            }

            Console.WriteLine($"[Grupo 3] Envío completado: {rutasArchivos.Count} archivo(s) a '{idDestinoConfig}'");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Grupo 3 ERROR] {ex.Message}");
            return false;
        }
    }

    private static DestinoConfig ObtenerConfiguracion(string idDestinoConfig)
    {
        string rutaConfig = Path.Combine(AppContext.BaseDirectory, ArchivoConfiguracion);

        if (!File.Exists(rutaConfig))
            throw new FileNotFoundException($"No se encontró el archivo de configuración de destinos: {rutaConfig}");

        var json = File.ReadAllText(rutaConfig);
        var destinos = JsonSerializer.Deserialize<Dictionary<string, DestinoConfig>>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (destinos == null || !destinos.TryGetValue(idDestinoConfig, out var config))
            throw new KeyNotFoundException($"No existe configuración de destino para '{idDestinoConfig}'");

        return config;
    }

    private static void EnviarPorFtp(List<string> rutasArchivos, DestinoConfig config)
    {
        using var cliente = new FtpClient(config.Host, config.Usuario, config.Contrasena, config.Puerto);
        cliente.Connect();

        foreach (var ruta in rutasArchivos)
        {
            string rutaRemota = CombinarRutaRemota(config.RutaRemota, Path.GetFileName(ruta));
            var estado = cliente.UploadFile(ruta, rutaRemota, FtpRemoteExists.Overwrite, true);

            if (estado is FtpStatus.Failed or FtpStatus.Skipped)
                throw new InvalidOperationException($"Fallo al subir '{ruta}' por FTP a '{rutaRemota}'");

            Console.WriteLine($"[Grupo 3] FTP OK: {Path.GetFileName(ruta)} -> {rutaRemota}");
        }
    }

    private static void EnviarPorSftp(List<string> rutasArchivos, DestinoConfig config)
    {
        using var cliente = new SftpClient(config.Host, config.Puerto, config.Usuario, config.Contrasena);
        cliente.Connect();

        if (!string.IsNullOrEmpty(config.RutaRemota) && !cliente.Exists(config.RutaRemota))
            cliente.CreateDirectory(config.RutaRemota);

        foreach (var ruta in rutasArchivos)
        {
            string rutaRemota = CombinarRutaRemota(config.RutaRemota, Path.GetFileName(ruta));

            using var flujo = File.OpenRead(ruta);
            cliente.UploadFile(flujo, rutaRemota, true);

            Console.WriteLine($"[Grupo 3] SFTP OK: {Path.GetFileName(ruta)} -> {rutaRemota}");
        }
    }

    private static string CombinarRutaRemota(string carpetaRemota, string nombreArchivo)
    {
        string carpeta = string.IsNullOrEmpty(carpetaRemota) ? string.Empty : carpetaRemota.TrimEnd('/');
        return string.IsNullOrEmpty(carpeta) ? nombreArchivo : $"{carpeta}/{nombreArchivo}";
    }

    private sealed class DestinoConfig
    {
        [JsonPropertyName("protocolo")]
        public string Protocolo { get; set; } = string.Empty;

        [JsonPropertyName("host")]
        public string Host { get; set; } = string.Empty;

        [JsonPropertyName("puerto")]
        public int Puerto { get; set; }

        [JsonPropertyName("usuario")]
        public string Usuario { get; set; } = string.Empty;

        [JsonPropertyName("contrasena")]
        public string Contrasena { get; set; } = string.Empty;

        [JsonPropertyName("rutaRemota")]
        public string RutaRemota { get; set; } = string.Empty;
    }
}
