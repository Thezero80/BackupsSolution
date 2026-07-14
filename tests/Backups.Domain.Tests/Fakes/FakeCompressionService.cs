namespace Backups.Domain.Tests.Fakes;

using System.Collections.Generic;
using Backups.Core.Ports.Out;

public class FakeCompressionService : ICompressionService
{
    private readonly List<string> _resultado;

    public FakeCompressionService(List<string> resultado)
    {
        _resultado = resultado;
    }

    public int LlamadasCount { get; private set; }

    public List<string> ComprimirYSegmentar(string rutaOrigen, string algoritmo, int limiteVolumenMb)
    {
        LlamadasCount++;
        return _resultado;
    }
}
