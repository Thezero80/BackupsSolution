namespace Backups.Adapters.Tests;

using System;
using System.Collections.Generic;
using System.IO;
using Backups.Infrastructure.Network;

/// <summary>
/// Pruebas complementarias a NetworkTransmissionAdapterTests: casos parametrizados
/// (Theory/InlineData) y evidencia de que host, puerto, usuario, contraseña y ruta
/// remota se leen siempre desde el archivo de configuración inyectado -nunca desde
/// constantes en el código- (regla del profesor: cero valores hardcodeados).
/// </summary>
public class NetworkTransmissionAdapterHardeningTests : IDisposable
{
    private readonly string _directorioTemporal;
    private readonly string _rutaArchivoValido;

    public NetworkTransmissionAdapterHardeningTests()
    {
        _directorioTemporal = Path.Combine(Path.GetTempPath(), "NetworkTransmissionAdapterHardeningTests_" + Guid.NewGuid());
        Directory.CreateDirectory(_directorioTemporal);

        _rutaArchivoValido = Path.Combine(_directorioTemporal, "archivo_comprimido.zip.part1");
        File.WriteAllText(_rutaArchivoValido, "contenido de prueba");
    }

    public void Dispose()
    {
        if (Directory.Exists(_directorioTemporal))
            Directory.Delete(_directorioTemporal, recursive: true);
    }

    private string EscribirConfig(string idDestino, string host, int puerto, string usuario, string contrasena, string rutaRemota)
    {
        string json = $$"""
            {
              "{{idDestino}}": { "Host": "{{host}}", "Puerto": {{puerto}}, "Usuario": "{{usuario}}", "Contrasena": "{{contrasena}}", "RutaRemota": "{{rutaRemota}}" }
            }
            """;
        string ruta = Path.Combine(_directorioTemporal, "destinos_" + Guid.NewGuid() + ".json");
        File.WriteAllText(ruta, json);
        return ruta;
    }

    [Theory]
    [InlineData("destino_a", "ftp.empresa-a.com", 21, "usuarioA", "claveA", "/respaldos/a/")]
    [InlineData("destino_b", "192.168.1.50", 2121, "usuarioB", "claveB", "/otra/ruta/")]
    [InlineData("destino_c", "backup.otra-empresa.net", 990, "root", "s3cr3t", "/")]
    public void EnviarArchivos_LeeHostPuertoYRutaDesdeConfiguracion_NoDesdeConstantes(
        string idDestino, string host, int puerto, string usuario, string contrasena, string rutaRemota)
    {
        string rutaConfig = EscribirConfig(idDestino, host, puerto, usuario, contrasena, rutaRemota);
        var uploader = new UploaderQueRegistraLlamadas();
        var adapter = new NetworkTransmissionAdapter(uploader, rutaConfig);

        bool resultado = adapter.EnviarArchivos(new List<string> { _rutaArchivoValido }, idDestino);

        Assert.True(resultado);
        var llamada = Assert.Single(uploader.LlamadasRecibidas);
        Assert.Equal(host, llamada.Host);
        Assert.Equal(puerto, llamada.Puerto);
        Assert.Equal(usuario, llamada.Usuario);
        Assert.Equal(contrasena, llamada.Contrasena);
        Assert.Equal(rutaRemota, llamada.RutaRemota);
    }

    [Fact]
    public void EnviarArchivos_Deberia_RetornarFalse_CuandoCredencialesSonInvalidas()
    {
        string rutaConfig = EscribirConfig("destino", "ftp.ejemplo.com", 21, "usuario", "clave-incorrecta", "/backups/");
        var uploaderQueRechazaCredenciales = new UploaderQueFalla(
            new System.Net.WebException("530 Not logged in: credenciales inválidas (simulado)."));
        var adapter = new NetworkTransmissionAdapter(uploaderQueRechazaCredenciales, rutaConfig);

        bool resultado = adapter.EnviarArchivos(new List<string> { _rutaArchivoValido }, "destino");

        Assert.False(resultado);
    }

    private sealed class UploaderQueRegistraLlamadas : IFtpUploader
    {
        public List<(string RutaArchivoLocal, string Host, int Puerto, string Usuario, string Contrasena, string RutaRemota)> LlamadasRecibidas { get; } = new();

        public void Subir(string rutaArchivoLocal, string host, int puerto, string usuario, string contrasena, string rutaRemota)
        {
            LlamadasRecibidas.Add((rutaArchivoLocal, host, puerto, usuario, contrasena, rutaRemota));
        }
    }

    private sealed class UploaderQueFalla : IFtpUploader
    {
        private readonly Exception _excepcion;

        public UploaderQueFalla(Exception excepcion)
        {
            _excepcion = excepcion;
        }

        public void Subir(string rutaArchivoLocal, string host, int puerto, string usuario, string contrasena, string rutaRemota)
            => throw _excepcion;
    }
}
