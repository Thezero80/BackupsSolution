using System.Collections.Generic;
namespace Backups.Core.Ports.Out;
public interface ITransmissionService
{
    bool EnviarArchivos(List<string> rutasArchivos, string idDestinoConfig);
}
