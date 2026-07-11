using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using Backups.Core.Ports.Out;

namespace Backups.Infrastructure.Compression;

public class CompressionAdapter : ICompressionService
{
    private const int TamanoBufferBytes = 81920;

    public List<string> ComprimirYSegmentar(string rutaOrigen, string algoritmo, int limiteVolumenMb)
    {
        if (!File.Exists(rutaOrigen))
            throw new FileNotFoundException($"No se encontró el archivo de origen: {rutaOrigen}");

        string rutaComprimidos = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WinBackup",
            "Comprimidos");

        if (!Directory.Exists(rutaComprimidos))
            Directory.CreateDirectory(rutaComprimidos);

        string destino = Path.Combine(rutaComprimidos, Path.GetFileNameWithoutExtension(rutaOrigen) + "_comprimido");

        string rutaComprimida = algoritmo.ToUpperInvariant() switch
        {
            "ZIP" => ComprimirZip(rutaOrigen, destino),
            "LZMA" => ComprimirConHerramientaExterna(rutaOrigen, destino, "7z", "a -t7z \"{0}\" \"{1}\"", ".7z"),
            "RAR" => ComprimirConHerramientaExterna(rutaOrigen, destino, "rar", "a \"{0}\" \"{1}\"", ".rar"),
            _ => throw new NotSupportedException($"Algoritmo de compresión no soportado: {algoritmo}"),
        };

        return Segmentar(rutaComprimida, limiteVolumenMb);
    }

    private static string ComprimirZip(string rutaOrigen, string destino)
    {
        string rutaZip = destino + ".zip";

        if (File.Exists(rutaZip))
            File.Delete(rutaZip);

        using (var zip = ZipFile.Open(rutaZip, ZipArchiveMode.Create))
        {
            zip.CreateEntryFromFile(rutaOrigen, Path.GetFileName(rutaOrigen), CompressionLevel.Optimal);
        }

        return rutaZip;
    }

    private static string ComprimirConHerramientaExterna(
        string rutaOrigen,
        string destino,
        string herramienta,
        string formatoArgumentos,
        string extension)
    {
        string rutaSalida = destino + extension;

        if (File.Exists(rutaSalida))
            File.Delete(rutaSalida);

        var info = new ProcessStartInfo
        {
            FileName = herramienta,
            Arguments = string.Format(formatoArgumentos, rutaSalida, rutaOrigen),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };

        try
        {
            using var proceso = Process.Start(info);
            if (proceso == null)
                throw new InvalidOperationException($"No se pudo iniciar el proceso '{herramienta}'.");

            proceso.WaitForExit();

            if (proceso.ExitCode != 0)
            {
                string error = proceso.StandardError.ReadToEnd();
                throw new InvalidOperationException($"'{herramienta}' finalizó con código {proceso.ExitCode}: {error}");
            }
        }
        catch (Win32Exception ex)
        {
            throw new InvalidOperationException(
                $"La herramienta '{herramienta}' no está disponible en el sistema. Instálala y agrégala al PATH.", ex);
        }

        return rutaSalida;
    }

    private static List<string> Segmentar(string rutaComprimida, int limiteVolumenMb)
    {
        try
        {
            long limiteBytes = (long)limiteVolumenMb * 1024 * 1024;
            var volumenes = new List<string>();

            using (var origen = File.OpenRead(rutaComprimida))
            {
                if (origen.Length <= limiteBytes)
                {
                    volumenes.Add(rutaComprimida);
                    return volumenes;
                }

                var buffer = new byte[TamanoBufferBytes];
                int numeroParte = 1;

                while (origen.Position < origen.Length)
                {
                    string rutaParte = $"{rutaComprimida}.part{numeroParte}";
                    long bytesEscritos = 0;

                    using (var parte = File.Create(rutaParte))
                    {
                        int leidos;
                        while (bytesEscritos < limiteBytes &&
                               (leidos = origen.Read(buffer, 0, (int)Math.Min(buffer.Length, limiteBytes - bytesEscritos))) > 0)
                        {
                            parte.Write(buffer, 0, leidos);
                            bytesEscritos += leidos;
                        }
                    }

                    volumenes.Add(rutaParte);
                    numeroParte++;
                }
            }

            File.Delete(rutaComprimida);
            return volumenes;
        }
        catch (Exception)
        {
            try
            {
                if (File.Exists(rutaComprimida))
                    File.Delete(rutaComprimida);
            }
            catch
            {
                // ignorar errores de limpieza
            }

            throw;
        }
    }
}
