namespace Backups.Domain.Tests.Fakes;

using System.Collections.Generic;
using Backups.Core.Ports.Out;

public class FakeTransmissionService : ITransmissionService
{
    private readonly bool _resultado;

    public FakeTransmissionService(bool resultado)
    {
        _resultado = resultado;
    }

    public bool FueInvocado { get; private set; }

    public bool EnviarArchivos(List<string> rutasArchivos, string idDestinoConfig)
    {
        FueInvocado = true;
        return _resultado;
    }
}
