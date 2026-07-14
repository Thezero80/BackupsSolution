namespace Backups.Core.Domain.Entities;

using System;

/// <summary>
/// Entidad que representa un registro de respaldo en el log.
/// Guardada en archivo control_backups.json.
/// </summary>
public class RegistroRespaldo
{
    /// <summary>
    /// ID único para este respaldo (UUID).
    /// </summary>
    public string BackupId { get; set; } = string.Empty;

    /// <summary>
    /// Estado del respaldo. Valores permitidos:
    /// - "OMITIDO_SIN_CAMBIOS"
    /// - "PROCESANDO"
    /// - "COMPRIMIENDO_VOLUMENES"
    /// - "ENVIANDO"
    /// - "COMPLETADO"
    /// - "FALLIDO"
    /// </summary>
    public string Estado { get; set; } = string.Empty;

    /// <summary>
    /// Mensaje descriptivo para el usuario.
    /// </summary>
    public string MensajeTexto { get; set; } = string.Empty;

    /// <summary>
    /// Fecha y hora exacta del registro.
    /// </summary>
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
}
