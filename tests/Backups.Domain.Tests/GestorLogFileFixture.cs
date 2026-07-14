namespace Backups.Domain.Tests;

using System;
using System.IO;

/// <summary>
/// GestorLog escribe siempre en %APPDATA%\WinBackup\control_backups.json (ruta fija,
/// no inyectable). Este fixture respalda cualquier archivo real existente antes de la
/// suite y lo restaura al finalizar, para no perder datos del usuario ni dejar residuos.
/// </summary>
public class GestorLogFileFixture : IDisposable
{
    private readonly string _rutaLog;
    private readonly string? _rutaBackup;
    private readonly bool _existiaArchivo;

    public GestorLogFileFixture()
    {
        _rutaLog = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WinBackup",
            "control_backups.json");

        _existiaArchivo = File.Exists(_rutaLog);
        if (_existiaArchivo)
        {
            _rutaBackup = _rutaLog + ".test-backup";
            File.Copy(_rutaLog, _rutaBackup, overwrite: true);
        }
    }

    public string RutaLog => _rutaLog;

    public void LimpiarArchivo()
    {
        if (File.Exists(_rutaLog))
        {
            File.Delete(_rutaLog);
        }
    }

    public void Dispose()
    {
        LimpiarArchivo();

        if (_existiaArchivo && _rutaBackup is not null)
        {
            File.Copy(_rutaBackup, _rutaLog, overwrite: true);
            File.Delete(_rutaBackup);
        }
    }
}

[CollectionDefinition("GestorLogFile")]
public class GestorLogFileCollection : ICollectionFixture<GestorLogFileFixture>
{
}
