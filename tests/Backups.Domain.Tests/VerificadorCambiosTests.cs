namespace Backups.Domain.Tests;

using System.IO;
using Backups.Core.Domain.Services;

public class VerificadorCambiosTests
{
    [Fact]
    public void CalcularHashArchivo_ArchivoInexistente_LanzaFileNotFoundException()
    {
        var verificador = new VerificadorCambios();
        var rutaInexistente = Path.Combine(Path.GetTempPath(), "no-existe-" + Path.GetRandomFileName());

        Assert.Throws<FileNotFoundException>(() => verificador.CalcularHashArchivo(rutaInexistente));
    }

    [Fact]
    public void CalcularHashArchivo_MismoContenido_GeneraElMismoHash()
    {
        var verificador = new VerificadorCambios();
        var archivo = Path.GetTempFileName();
        try
        {
            File.WriteAllText(archivo, "contenido de prueba");

            var hash1 = verificador.CalcularHashArchivo(archivo);
            var hash2 = verificador.CalcularHashArchivo(archivo);

            Assert.Equal(hash1, hash2);
        }
        finally
        {
            File.Delete(archivo);
        }
    }

    [Fact]
    public void CalcularHashArchivo_ContenidoDiferente_GeneraHashDiferente()
    {
        var verificador = new VerificadorCambios();
        var archivo = Path.GetTempFileName();
        try
        {
            File.WriteAllText(archivo, "contenido A");
            var hashA = verificador.CalcularHashArchivo(archivo);

            File.WriteAllText(archivo, "contenido B");
            var hashB = verificador.CalcularHashArchivo(archivo);

            Assert.NotEqual(hashA, hashB);
        }
        finally
        {
            File.Delete(archivo);
        }
    }

    [Theory]
    [InlineData("", "cualquier-hash", true)] // sin hash previo => se considera cambiado
    [InlineData("abc123", "ABC123", false)] // mismo hash, distinta capitalización => sin cambios
    [InlineData("abc123", "def456", true)] // hashes distintos => cambiado
    [InlineData("ABC123", "abc123", false)] // orden inverso de mayúsculas/minúsculas
    public void ArchivoFueCambiado_ComparaHashesCorrectamente(string hashAnterior, string hashActual, bool esperado)
    {
        var verificador = new VerificadorCambios();

        Assert.Equal(esperado, verificador.ArchivoFueCambiado(hashAnterior, hashActual));
    }
}
