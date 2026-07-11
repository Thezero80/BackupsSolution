using System;
using System.Collections.Generic;
using System.IO;
using Backups.Infrastructure.Network;

namespace Backups.Adapters.Tests;

public class NetworkTransmissionAdapterTests : IDisposable
{
    private const string ConfigDestinoValido = """
        {
          "ftp_servidor_1": { "Host": "ftp.ejemplo.com", "Puerto": 21, "Usuario": "u", "Contrasena": "p", "RutaRemota": "/backups/" }
        }
        """;

    private readonly string _directorioTemporal;
    private readonly string _rutaArchivoValido;

    public NetworkTransmissionAdapterTests()
    {
        _directorioTemporal = Path.Combine(Path.GetTempPath(), "NetworkTransmissionAdapterTests_" + Guid.NewGuid());
        Directory.CreateDirectory(_directorioTemporal);

        _rutaArchivoValido = Path.Combine(_directorioTemporal, "archivo_comprimido.zip.part1");
        File.WriteAllText(_rutaArchivoValido, "contenido de prueba");
    }

    public void Dispose()
    {
        if (Directory.Exists(_directorioTemporal))
            Directory.Delete(_directorioTemporal, recursive: true);
    }

    private string EscribirConfig(string contenidoJson)
    {
        string ruta = Path.Combine(_directorioTemporal, "destinos_" + Guid.NewGuid() + ".json");
        File.WriteAllText(ruta, contenidoJson);
        return ruta;
    }

    [Fact]
    public void EnviarArchivos_Deberia_RetornarFalse_CuandoConfigNoExiste()
    {
        string rutaConfigInexistente = Path.Combine(_directorioTemporal, "destinos_no_existe.json");
        var adapter = new NetworkTransmissionAdapter(new UploaderQueNuncaDebeLlamarse(), rutaConfigInexistente);

        bool resultado = adapter.EnviarArchivos(new List<string> { _rutaArchivoValido }, "ftp_servidor_1");

        Assert.False(resultado);
    }

    [Fact]
    public void EnviarArchivos_Deberia_RetornarFalse_CuandoIdDestinoNoExisteEnConfig()
    {
        string rutaConfig = EscribirConfig(ConfigDestinoValido);
        var adapter = new NetworkTransmissionAdapter(new UploaderQueNuncaDebeLlamarse(), rutaConfig);

        bool resultado = adapter.EnviarArchivos(new List<string> { _rutaArchivoValido }, "id_que_no_existe");

        Assert.False(resultado);
    }

    [Fact]
    public void EnviarArchivos_Deberia_RetornarFalse_CuandoArchivoOrigenNoExiste()
    {
        string rutaConfig = EscribirConfig(ConfigDestinoValido);
        string rutaInexistente = Path.Combine(_directorioTemporal, "no_existe.zip.part1");
        var adapter = new NetworkTransmissionAdapter(new UploaderQueNuncaDebeLlamarse(), rutaConfig);

        bool resultado = adapter.EnviarArchivos(new List<string> { rutaInexistente }, "ftp_servidor_1");

        Assert.False(resultado);
    }

    [Fact]
    public void EnviarArchivos_Deberia_RetornarFalse_CuandoUnaDeVariasRutasNoExiste()
    {
        string rutaConfig = EscribirConfig(ConfigDestinoValido);
        string rutaInexistente = Path.Combine(_directorioTemporal, "parte_faltante.zip.part2");
        var adapter = new NetworkTransmissionAdapter(new UploaderQueNuncaDebeLlamarse(), rutaConfig);

        bool resultado = adapter.EnviarArchivos(new List<string> { _rutaArchivoValido, rutaInexistente }, "ftp_servidor_1");

        Assert.False(resultado);
    }

    [Fact]
    public void EnviarArchivos_Deberia_RetornarFalse_CuandoHostEsInalcanzable()
    {
        string rutaConfig = EscribirConfig(ConfigDestinoValido);
        var uploaderQueFalla = new UploaderSimulado(lanzarError: true);
        var adapter = new NetworkTransmissionAdapter(uploaderQueFalla, rutaConfig);

        bool resultado = adapter.EnviarArchivos(new List<string> { _rutaArchivoValido }, "ftp_servidor_1");

        Assert.False(resultado);
    }

    [Fact]
    public void EnviarArchivos_Deberia_RetornarTrue_CuandoTodoEsCorrecto()
    {
        string rutaConfig = EscribirConfig(ConfigDestinoValido);
        var uploaderSimulado = new UploaderSimulado(lanzarError: false);
        var adapter = new NetworkTransmissionAdapter(uploaderSimulado, rutaConfig);

        bool resultado = adapter.EnviarArchivos(new List<string> { _rutaArchivoValido }, "ftp_servidor_1");

        Assert.True(resultado);
        var llamada = Assert.Single(uploaderSimulado.LlamadasRecibidas);
        Assert.Equal(_rutaArchivoValido, llamada.RutaArchivoLocal);
        Assert.Equal("ftp.ejemplo.com", llamada.Host);
        Assert.Equal(21, llamada.Puerto);
        Assert.Equal("/backups/", llamada.RutaRemota);
    }

    private sealed class UploaderQueNuncaDebeLlamarse : IFtpUploader
    {
        public void Subir(string rutaArchivoLocal, string host, int puerto, string usuario, string contrasena, string rutaRemota)
            => throw new InvalidOperationException("No debería intentarse la subida en este escenario.");
    }

    private sealed class UploaderSimulado : IFtpUploader
    {
        private readonly bool _lanzarError;

        public UploaderSimulado(bool lanzarError)
        {
            _lanzarError = lanzarError;
        }

        public List<(string RutaArchivoLocal, string Host, int Puerto, string Usuario, string Contrasena, string RutaRemota)> LlamadasRecibidas { get; } = new();

        public void Subir(string rutaArchivoLocal, string host, int puerto, string usuario, string contrasena, string rutaRemota)
        {
            if (_lanzarError)
                throw new System.Net.WebException("No se pudo conectar al servidor remoto (simulado).");

            LlamadasRecibidas.Add((rutaArchivoLocal, host, puerto, usuario, contrasena, rutaRemota));
        }
    }
}
