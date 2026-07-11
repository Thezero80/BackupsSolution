using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using Backups.Core.Ports.Out;

namespace Backups.Infrastructure.Network;

internal interface IFtpUploader
{
    void Subir(string rutaArchivoLocal, string host, int puerto, string usuario, string contrasena, string rutaRemota);
}

public class NetworkTransmissionAdapter : ITransmissionService
{
    private static readonly string RutaConfiguracionPorDefecto = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "WinBackup",
        "destinos.json");

    private readonly IFtpUploader _ftpUploader;
    private readonly string _rutaConfiguracion;

    public NetworkTransmissionAdapter()
        : this(new FtpWebRequestUploader(), RutaConfiguracionPorDefecto)
    {
    }

    internal NetworkTransmissionAdapter(IFtpUploader ftpUploader, string rutaConfiguracion)
    {
        _ftpUploader = ftpUploader;
        _rutaConfiguracion = rutaConfiguracion;
    }

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

            foreach (var ruta in rutasArchivos)
            {
                _ftpUploader.Subir(ruta, config.Host, config.Puerto, config.Usuario, config.Contrasena, config.RutaRemota);
                Console.WriteLine($"[Grupo 3] Transmitido: {Path.GetFileName(ruta)} -> {config.Host}{config.RutaRemota}");
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Grupo 3 ERROR] {ex.Message}");
            return false;
        }
    }

    private ConfigDestino ObtenerConfiguracion(string idDestinoConfig)
    {
        if (!File.Exists(_rutaConfiguracion))
        {
            throw new FileNotFoundException(
                $"No existe el archivo de configuración de destinos: {_rutaConfiguracion}. " +
                "Debe contener un objeto JSON con la forma " +
                "{ \"idDestinoConfig\": { \"Host\": \"...\", \"Puerto\": 21, \"Usuario\": \"...\", \"Contrasena\": \"...\", \"RutaRemota\": \"/backups/\" } }.");
        }

        string json = File.ReadAllText(_rutaConfiguracion);
        var destinos = JsonSerializer.Deserialize<Dictionary<string, ConfigDestino>>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new Dictionary<string, ConfigDestino>();

        if (!destinos.TryGetValue(idDestinoConfig, out var config))
            throw new KeyNotFoundException($"No se encontró configuración para el destino: {idDestinoConfig}");

        return config;
    }

    private class ConfigDestino
    {
        public string Host { get; set; } = string.Empty;
        public int Puerto { get; set; } = 21;
        public string Usuario { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
        public string RutaRemota { get; set; } = "/";
    }
}

#pragma warning disable SYSLIB0014 // FtpWebRequest es la única opción FTP sin dependencias externas en .NET
internal class FtpWebRequestUploader : IFtpUploader
{
    public void Subir(string rutaArchivoLocal, string host, int puerto, string usuario, string contrasena, string rutaRemota)
    {
        string nombreArchivo = Path.GetFileName(rutaArchivoLocal);
        string uriDestino = $"ftp://{host}:{puerto}{rutaRemota.TrimEnd('/')}/{nombreArchivo}";

        var request = (FtpWebRequest)WebRequest.Create(uriDestino);
        request.Method = WebRequestMethods.Ftp.UploadFile;
        request.Credentials = new NetworkCredential(usuario, contrasena);
        request.UseBinary = true;
        request.UsePassive = true;

        using (var streamOrigen = File.OpenRead(rutaArchivoLocal))
        using (var streamDestino = request.GetRequestStream())
        {
            streamOrigen.CopyTo(streamDestino);
        }

        using var response = (FtpWebResponse)request.GetResponse();
        if (response.StatusCode != FtpStatusCode.ClosingData && response.StatusCode != FtpStatusCode.FileActionOK)
            throw new InvalidOperationException($"FTP respondió con estado inesperado: {response.StatusCode} para {nombreArchivo}");
    }
}
#pragma warning restore SYSLIB0014
