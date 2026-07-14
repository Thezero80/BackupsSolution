namespace Backups.Adapters.Tests;

using Backups.Infrastructure.Compression;

/// <summary>
/// CompressionAdapter (Grupo 2) es hoy un stub: no implementa ZIP/LZMA/RAR ni
/// segmentación real, solo devuelve una lista fija. Estas pruebas cubren
/// únicamente el contrato actual del stub (ICompressionService). Cuando Grupo 2
/// implemente la lógica real (incluyendo los casos de 7z/rar ausentes del PATH),
/// esta clase debe reemplazarse por pruebas del comportamiento real.
/// </summary>
public class CompressionAdapterTests
{
    [Fact]
    public void ComprimirYSegmentar_Stub_DevuelveListaNoVaciaConAlMenosUnVolumen()
    {
        var adapter = new CompressionAdapter();

        var resultado = adapter.ComprimirYSegmentar("cualquier-ruta.txt", "ZIP", 100);

        Assert.NotNull(resultado);
        Assert.NotEmpty(resultado);
        Assert.All(resultado, ruta => Assert.False(string.IsNullOrWhiteSpace(ruta)));
    }
}
