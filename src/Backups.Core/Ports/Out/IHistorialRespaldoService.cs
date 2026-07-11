namespace Backups.Core.Ports.Out;

using System.Collections.Generic;
using Backups.Core.Domain.Entities;

/// <summary>
/// Puerto de salida para acceder al historial de respaldos.
/// Implementa: GestorLog
/// Usa: Grupo 4 (UI) para mostrar historial
/// </summary>
public interface IHistorialRespaldoService
{
    /// <summary>
    /// Obtiene la lista completa de registros de respaldos.
    /// </summary>
    /// <returns>Lista de RegistroRespaldo, nunca null (puede estar vacía)</returns>
    List<RegistroRespaldo> ObtenerHistorial();
}
