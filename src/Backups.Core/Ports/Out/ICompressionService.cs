using System.Collections.Generic;
namespace Backups.Core.Ports.Out;
public interface ICompressionService
{
    List<string> ComprimirYSegmentar(string rutaOrigen, string algoritmo, int limiteVolumenMb);
}
