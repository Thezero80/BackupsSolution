using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Backups.Infrastructure.Compression;
using Xunit;

namespace Backups.Infrastructure.Compression.Tests;

public class CompressionAdapterTests : IDisposable
{
    private readonly CompressionAdapter _sut = new();
    private readonly string _rutaOrigenTemporal;
    private readonly string _carpetaComprimidos;

    public CompressionAdapterTests()
    {
        _rutaOrigenTemporal = Path.Combine(Path.GetTempPath(), $"origen_{Guid.NewGuid():N}.txt");
        _carpetaComprimidos = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WinBackup",
            "Comprimidos");
    }

    public void Dispose()
    {
        if (File.Exists(_rutaOrigenTemporal))
            File.Delete(_rutaOrigenTemporal);

        if (Directory.Exists(_carpetaComprimidos))
        {
            foreach (var archivo in Directory.GetFiles(_carpetaComprimidos, "origen_*"))
            {
                try { File.Delete(archivo); }
                catch { /* ignorar residuos de limpieza */ }
            }
        }
    }

    private void CrearArchivoOrigen(int tamanoBytes, bool contenidoAleatorio = false)
    {
        var datos = new byte[tamanoBytes];
        if (contenidoAleatorio)
            new Random(42).NextBytes(datos); // incompresible: fuerza a que Segmentar actúe de verdad
        File.WriteAllBytes(_rutaOrigenTemporal, datos);
    }

    [Fact]
    public void ComprimirYSegmentar_ArchivoOrigenNoExiste_LanzaFileNotFoundException()
    {
        string rutaInexistente = Path.Combine(Path.GetTempPath(), $"no_existe_{Guid.NewGuid():N}.dbf");

        var excepcion = Assert.Throws<FileNotFoundException>(() =>
            _sut.ComprimirYSegmentar(rutaInexistente, "ZIP", 100));

        Assert.Contains(rutaInexistente, excepcion.Message);
    }

    [Theory]
    [InlineData("GZIP")]
    [InlineData("TAR")]
    [InlineData("")]
    public void ComprimirYSegmentar_AlgoritmoNoSoportado_LanzaNotSupportedException(string algoritmo)
    {
        CrearArchivoOrigen(1024);

        Assert.Throws<NotSupportedException>(() =>
            _sut.ComprimirYSegmentar(_rutaOrigenTemporal, algoritmo, 100));
    }

    [Fact]
    public void ComprimirYSegmentar_ZipArchivoPequeno_GeneraUnSoloVolumenDentroDelLimite()
    {
        CrearArchivoOrigen(10 * 1024); // 10 KB

        var volumenes = _sut.ComprimirYSegmentar(_rutaOrigenTemporal, "ZIP", 100);

        Assert.Single(volumenes);
        Assert.True(File.Exists(volumenes[0]));
        Assert.EndsWith(".zip", volumenes[0]);
    }

    [Fact]
    public void ComprimirYSegmentar_AlgoritmoEnMinusculas_FuncionaIgualQueEnMayusculas()
    {
        CrearArchivoOrigen(5 * 1024);

        var volumenes = _sut.ComprimirYSegmentar(_rutaOrigenTemporal, "zip", 100);

        Assert.Single(volumenes);
        Assert.True(File.Exists(volumenes[0]));
    }

    [Fact]
    public void ComprimirYSegmentar_ArchivoJustoEnElLimite_NoSegmenta()
    {
        CrearArchivoOrigen(1024); // 1 KB, muy por debajo del límite
        int limiteVolumenMb = 1;

        var volumenes = _sut.ComprimirYSegmentar(_rutaOrigenTemporal, "ZIP", limiteVolumenMb);

        Assert.Single(volumenes);
        Assert.DoesNotContain(".part", volumenes[0]);
    }

    [Fact]
    public void ComprimirYSegmentar_ArchivoSuperaElLimite_SegmentaEnMultiplesVolumenes()
    {
        CrearArchivoOrigen(5 * 1024 * 1024, contenidoAleatorio: true); // 5 MB incompresibles
        int limiteVolumenMb = 1; // fuerza al menos 5 partes

        var volumenes = _sut.ComprimirYSegmentar(_rutaOrigenTemporal, "ZIP", limiteVolumenMb);

        Assert.True(volumenes.Count > 1, "Se esperaban múltiples volúmenes al superar el límite.");

        foreach (var parte in volumenes)
        {
            Assert.True(File.Exists(parte));
            var info = new FileInfo(parte);
            Assert.True(
                info.Length <= limiteVolumenMb * 1024L * 1024L,
                $"La parte {parte} pesa {info.Length} bytes y supera el límite configurado.");
            Assert.Contains(".part", parte);
        }
    }

    [Fact]
    public void ComprimirYSegmentar_LZMA_SinHerramienta7zInstalada_LanzaInvalidOperationException()
    {
        if (HerramientaDisponible("7z"))
            return; // entorno con 7z en el PATH: este escenario de fallo no aplica aquí.

        CrearArchivoOrigen(1024);

        var excepcion = Assert.Throws<InvalidOperationException>(() =>
            _sut.ComprimirYSegmentar(_rutaOrigenTemporal, "LZMA", 100));

        Assert.Contains("7z", excepcion.Message);
    }

    [Fact]
    public void ComprimirYSegmentar_RAR_SinHerramientaRarInstalada_LanzaInvalidOperationException()
    {
        if (HerramientaDisponible("rar"))
            return; // entorno con rar en el PATH: este escenario de fallo no aplica aquí.

        CrearArchivoOrigen(1024);

        var excepcion = Assert.Throws<InvalidOperationException>(() =>
            _sut.ComprimirYSegmentar(_rutaOrigenTemporal, "RAR", 100));

        Assert.Contains("rar", excepcion.Message);
    }

    private static bool HerramientaDisponible(string nombreHerramienta)
    {
        try
        {
            string comandoBusqueda = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "where" : "which";
            var info = new ProcessStartInfo
            {
                FileName = comandoBusqueda,
                Arguments = nombreHerramienta,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };
            using var proceso = Process.Start(info);
            proceso!.WaitForExit();
            return proceso.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
