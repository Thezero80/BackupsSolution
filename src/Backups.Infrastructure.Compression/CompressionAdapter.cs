using System;
using System.Collections.Generic;
using Backups.Core.Ports.Out;
namespace Backups.Infrastructure.Compression;
public class CompressionAdapter : ICompressionService
{
    public List<string> ComprimirYSegmentar(string rutaOrigen, string algoritmo, int limiteVolumenMb)
    {
        Console.WriteLine($"[Grupo 2] Comprimiendo {rutaOrigen} usando {algoritmo} con lÃ­mite de {limiteVolumenMb}MB...");
        return new List<string> { "temporal_archivo.part1.zip" };
    }
}
