namespace Backups.Domain.Tests;

using System;
using System.Collections.Generic;
using System.IO;
using Backups.Core.Application.UseCases;
using Backups.Core.Domain.Entities;
using Backups.Core.Domain.Services;
using Backups.Domain.Tests.Fakes;

[Collection("GestorLogFile")]
public class EjecutarRespaldoUseCaseTests : IDisposable
{
    private readonly string _archivoOrigen;
    private readonly VerificadorCambios _verificador = new();

    public EjecutarRespaldoUseCaseTests(GestorLogFileFixture fixture)
    {
        fixture.LimpiarArchivo();
        _archivoOrigen = Path.GetTempFileName();
        File.WriteAllText(_archivoOrigen, "contenido inicial");
    }

    public void Dispose()
    {
        if (File.Exists(_archivoOrigen))
        {
            File.Delete(_archivoOrigen);
        }
    }

    [Fact]
    public void Ejecutar_HashConocidoVacio_ComprimeTransmiteYRegistraCompletado()
    {
        var compresion = new FakeCompressionService(new List<string> { "volumen1.zip" });
        var transmision = new FakeTransmissionService(true);
        var gestorLog = new GestorLog();
        var useCase = new EjecutarRespaldoUseCase(_verificador, gestorLog, compresion, transmision);
        var solicitud = new SolicitudRespaldo { RutaOrigen = _archivoOrigen, NombreCopia = "test", UltimoHashConocido = string.Empty };

        useCase.Ejecutar(solicitud);

        Assert.Equal(1, compresion.LlamadasCount);
        Assert.True(transmision.FueInvocado);
        var historial = gestorLog.ObtenerHistorial();
        Assert.Contains(historial, r => r.Estado == "COMPLETADO");
    }

    [Fact]
    public void Ejecutar_HashConocidoIgualAlActual_OmiteYNoComprimeNiTransmite()
    {
        var hashActual = _verificador.CalcularHashArchivo(_archivoOrigen);
        var compresion = new FakeCompressionService(new List<string> { "volumen1.zip" });
        var transmision = new FakeTransmissionService(true);
        var gestorLog = new GestorLog();
        var useCase = new EjecutarRespaldoUseCase(_verificador, gestorLog, compresion, transmision);
        var solicitud = new SolicitudRespaldo { RutaOrigen = _archivoOrigen, NombreCopia = "test", UltimoHashConocido = hashActual };

        useCase.Ejecutar(solicitud);

        Assert.Equal(0, compresion.LlamadasCount);
        Assert.False(transmision.FueInvocado);
        var historial = gestorLog.ObtenerHistorial();
        Assert.Contains(historial, r => r.Estado == "OMITIDO_SIN_CAMBIOS");
    }

    [Fact]
    public void Ejecutar_TransmisionFalla_RegistraFallido()
    {
        var compresion = new FakeCompressionService(new List<string> { "volumen1.zip" });
        var transmision = new FakeTransmissionService(false);
        var gestorLog = new GestorLog();
        var useCase = new EjecutarRespaldoUseCase(_verificador, gestorLog, compresion, transmision);
        var solicitud = new SolicitudRespaldo { RutaOrigen = _archivoOrigen, NombreCopia = "test", UltimoHashConocido = string.Empty };

        useCase.Ejecutar(solicitud);

        var historial = gestorLog.ObtenerHistorial();
        Assert.Contains(historial, r => r.Estado == "FALLIDO");
    }

    [Fact]
    public void Ejecutar_CompresionRetornaListaVacia_RegistraFallidoYNoTransmite()
    {
        var compresion = new FakeCompressionService(new List<string>());
        var transmision = new FakeTransmissionService(true);
        var gestorLog = new GestorLog();
        var useCase = new EjecutarRespaldoUseCase(_verificador, gestorLog, compresion, transmision);
        var solicitud = new SolicitudRespaldo { RutaOrigen = _archivoOrigen, NombreCopia = "test", UltimoHashConocido = string.Empty };

        useCase.Ejecutar(solicitud);

        Assert.False(transmision.FueInvocado);
        var historial = gestorLog.ObtenerHistorial();
        Assert.Contains(historial, r => r.Estado == "FALLIDO");
    }

    [Fact]
    public void Ejecutar_ArchivoOrigenInexistente_RegistraFallidoSinLanzarExcepcion()
    {
        var compresion = new FakeCompressionService(new List<string> { "volumen1.zip" });
        var transmision = new FakeTransmissionService(true);
        var gestorLog = new GestorLog();
        var useCase = new EjecutarRespaldoUseCase(_verificador, gestorLog, compresion, transmision);
        var rutaInexistente = Path.Combine(Path.GetTempPath(), "no-existe-" + Path.GetRandomFileName());
        var solicitud = new SolicitudRespaldo { RutaOrigen = rutaInexistente, NombreCopia = "test" };

        var exception = Record.Exception(() => useCase.Ejecutar(solicitud));

        Assert.Null(exception);
        Assert.Equal(0, compresion.LlamadasCount);
        var historial = gestorLog.ObtenerHistorial();
        Assert.Contains(historial, r => r.Estado == "FALLIDO");
    }
}
