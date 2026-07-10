namespace Backups.Core.Domain.Entities;
public class SolicitudRespaldo
{
    public string NombreCopia { get; set; } = string.Empty;
    public string TipoDisparo { get; set; } = "TIMER"; 
    public string RutaOrigen { get; set; } = string.Empty;
    public string UltimoHashConocido { get; set; } = string.Empty;
    public string AlgoritmoCompresion { get; set; } = "ZIP"; 
    public int LimiteVolumenMb { get; set; }
    public string IdDestinoConfig { get; set; } = string.Empty;
}
