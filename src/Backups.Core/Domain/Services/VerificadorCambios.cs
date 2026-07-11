namespace Backups.Core.Domain.Services;

using System;
using System.IO;
using System.Security.Cryptography;

/// <summary>
/// Servicio para verificar cambios en archivos usando hash SHA256.
/// Responsabilidad: Calcular y comparar hashes para detectar si un archivo cambió.
/// </summary>
public class VerificadorCambios
{
    /// <summary>
    /// Calcula el hash SHA256 de un archivo.
    /// </summary>
    /// <param name="rutaArchivo">Ruta completa del archivo</param>
    /// <returns>Hash hexadecimal en minúsculas</returns>
    /// <exception cref="FileNotFoundException">Si el archivo no existe</exception>
    public string CalcularHashArchivo(string rutaArchivo)
    {
        if (!File.Exists(rutaArchivo))
        {
            throw new FileNotFoundException($"No se encontró el archivo a verificar: {rutaArchivo}", rutaArchivo);
        }

        using var stream = File.OpenRead(rutaArchivo);
        using var sha256 = SHA256.Create();
        byte[] hashBytes = sha256.ComputeHash(stream);

        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    /// <summary>
    /// Compara dos hashes para determinar si un archivo cambió.
    /// </summary>
    /// <param name="hashAnterior">Hash guardado previamente (puede ser vacío)</param>
    /// <param name="hashActual">Hash calculado ahora</param>
    /// <returns>true si son diferentes (archivo cambió), false si son iguales</returns>
    public bool ArchivoFueCambiado(string hashAnterior, string hashActual)
    {
        if (string.IsNullOrEmpty(hashAnterior))
        {
            return true;
        }

        return !string.Equals(hashAnterior, hashActual, StringComparison.OrdinalIgnoreCase);
    }
}
