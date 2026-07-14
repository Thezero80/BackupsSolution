namespace Backups.Core.Domain.Services;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Backups.Core.Domain.Entities;
using Backups.Core.Ports.Out;

/// <summary>
/// Gestor de logging persistente.
/// Responsabilidad: Leer/escribir registros de respaldos en archivo JSON.
/// Archivo: control_backups.json (en carpeta de aplicación)
/// </summary>
public class GestorLog : IHistorialRespaldoService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
    };

    private readonly string _rutaLog;

    public GestorLog()
    {
        _rutaLog = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WinBackup",
            "control_backups.json"
        );

        var carpeta = Path.GetDirectoryName(_rutaLog);
        if (!string.IsNullOrEmpty(carpeta) && !Directory.Exists(carpeta))
        {
            Directory.CreateDirectory(carpeta);
        }
    }

    /// <summary>
    /// Registra un evento de respaldo en el archivo JSON.
    /// </summary>
    /// <param name="registro">Objeto RegistroRespaldo con todos los datos</param>
    public void RegistrarEvento(RegistroRespaldo registro)
    {
        List<RegistroRespaldo> historial = ObtenerHistorial();
        historial.Add(registro);

        string json = JsonSerializer.Serialize(historial, SerializerOptions);
        File.WriteAllText(_rutaLog, json);
    }

    /// <summary>
    /// Obtiene todos los registros de respaldos guardados.
    /// </summary>
    /// <returns>Lista de RegistroRespaldo (puede estar vacía)</returns>
    public List<RegistroRespaldo> ObtenerHistorial()
    {
        if (!File.Exists(_rutaLog))
        {
            return new List<RegistroRespaldo>();
        }

        string json = File.ReadAllText(_rutaLog);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<RegistroRespaldo>();
        }

        return JsonSerializer.Deserialize<List<RegistroRespaldo>>(json, SerializerOptions)
               ?? new List<RegistroRespaldo>();
    }
}
