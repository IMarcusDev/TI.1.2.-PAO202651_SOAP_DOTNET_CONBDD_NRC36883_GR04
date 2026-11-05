using System.IO;
using ec.edu.monster.config;

namespace ec.edu.monster.pruebas;

public class ConfiguracionTest
{
    [Fact]
    public void UrlBaseUsesHostAndPuerto()
        => Assert.Equal("http://192.168.1.20:5171/LoginWebService.svc",
            new Configuracion(" 192.168.1.20 ", 5171).Url(RutasSoap.ServicioLogin));

    [Fact]
    public void TheDefaultsPointAtLocalhostAndPort5171()
    {
        var configuracion = new Configuracion();

        Assert.Equal("localhost", configuracion.Host);
        Assert.Equal(5171, configuracion.Puerto);
        Assert.Equal("http://localhost:5171/", configuracion.UrlBase);
    }

    [Fact]
    public void ReadsTheServidorSectionFromTheFile()
    {
        string archivo = ArchivoTemporal("{\"Servidor\":{\"Host\":\"10.0.0.5\",\"Puerto\":5180}}");
        try
        {
            Configuracion configuracion = Configuracion.Cargar(archivo);

            Assert.Equal("10.0.0.5", configuracion.Host);
            Assert.Equal(5180, configuracion.Puerto);
            Assert.Equal("http://10.0.0.5:5180/MovimientoWebService.svc",
                configuracion.Url(RutasSoap.ServicioMovimiento));
        }
        finally
        {
            File.Delete(archivo);
        }
    }

    [Fact]
    public void AMissingFileFallsBackToTheDefaults()
        => Assert.Equal("http://localhost:5171/LoginWebService.svc",
            Configuracion.Cargar(Path.Combine(Path.GetTempPath(), "eurekabank-no-existe.json"))
                .Url(RutasSoap.ServicioLogin));

    [Fact]
    public void AFileWithoutTheServidorSectionFallsBackToTheDefaults()
    {
        string archivo = ArchivoTemporal("{\"Otro\":true}");
        try
        {
            Assert.Equal(5171, Configuracion.Cargar(archivo).Puerto);
        }
        finally
        {
            File.Delete(archivo);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(70000)]
    public void RejectsAnOutOfRangePort(int puerto)
        => Assert.Throws<ArgumentOutOfRangeException>(() => new Configuracion("localhost", puerto));

    private static string ArchivoTemporal(string contenido)
    {
        string archivo = Path.Combine(Path.GetTempPath(), $"eurekabank-{Guid.NewGuid():N}.json");
        File.WriteAllText(archivo, contenido);
        return archivo;
    }
}
