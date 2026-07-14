namespace Backups.Domain.Tests;

using System.IO;
using System.Text.Json;
using Backups.Core.Domain.Entities;
using Backups.Core.Domain.Services;

[Collection("GestorLogFile")]
public class GestorLogTests
{
    private readonly GestorLogFileFixture _fixture;

    public GestorLogTests(GestorLogFileFixture fixture)
    {
        _fixture = fixture;
        _fixture.LimpiarArchivo();
    }

    [Fact]
    public void ObtenerHistorial_SinArchivoPrevio_RetornaListaVacia()
    {
        var gestor = new GestorLog();

        var historial = gestor.ObtenerHistorial();

        Assert.Empty(historial);
    }

    [Fact]
    public void ObtenerHistorial_ArchivoVacio_RetornaListaVacia()
    {
        File.WriteAllText(_fixture.RutaLog, string.Empty);
        var gestor = new GestorLog();

        var historial = gestor.ObtenerHistorial();

        Assert.Empty(historial);
    }

    [Fact]
    public void ObtenerHistorial_ArchivoConJsonCorrupto_LanzaJsonException()
    {
        File.WriteAllText(_fixture.RutaLog, "{ esto no es json valido ][");
        var gestor = new GestorLog();

        Assert.Throws<JsonException>(() => gestor.ObtenerHistorial());
    }

    [Fact]
    public void RegistrarEvento_AgregaAlHistorial()
    {
        var gestor = new GestorLog();
        var registro = new RegistroRespaldo
        {
            BackupId = "abc",
            Estado = "COMPLETADO",
            MensajeTexto = "Respaldo completado",
        };

        gestor.RegistrarEvento(registro);
        var historial = gestor.ObtenerHistorial();

        var unico = Assert.Single(historial);
        Assert.Equal("abc", unico.BackupId);
        Assert.Equal("COMPLETADO", unico.Estado);
    }

    [Fact]
    public void RegistrarEvento_MultiplesEventos_MantieneOrdenDeInsercion()
    {
        var gestor = new GestorLog();
        gestor.RegistrarEvento(new RegistroRespaldo { BackupId = "1", Estado = "PROCESANDO" });
        gestor.RegistrarEvento(new RegistroRespaldo { BackupId = "2", Estado = "COMPLETADO" });

        var historial = gestor.ObtenerHistorial();

        Assert.Equal(2, historial.Count);
        Assert.Equal("1", historial[0].BackupId);
        Assert.Equal("2", historial[1].BackupId);
    }
}
