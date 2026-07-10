using System;
using System.Collections.Generic;
using Backups.Core.Ports.Out;
namespace Backups.Infrastructure.Network;
public class NetworkTransmissionAdapter : ITransmissionService
{
    public bool EnviarArchivos(List<string> rutasArchivos, string idDestinoConfig)
    {
        Console.WriteLine($"[Grupo 3] Enviando {rutasArchivos.Count} partes al destino {idDestinoConfig}...");
        foreach (var archivo in rutasArchivos) { Console.WriteLine($"[Grupo 3] Transmitiendo: {archivo}"); }
        return true;
    }
}
