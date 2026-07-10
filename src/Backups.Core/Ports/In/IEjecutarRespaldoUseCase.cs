using Backups.Core.Domain.Entities;
namespace Backups.Core.Ports.In;
public interface IEjecutarRespaldoUseCase
{
    void Ejecutar(SolicitudRespaldo solicitud);
}
