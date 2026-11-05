using ec.edu.monster.fakes;
using ec.edu.monster.modelos.servicios;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace ec.edu.monster.config;

public class ConfiguracionTest
{
    [Fact]
    public void UrlBaseUsesHostAndPuerto()
        => Assert.Equal("http://192.168.1.20:5171/LoginWebService.svc",
            new ServidorOptions { Host = " 192.168.1.20 ", Puerto = 5171 }.Url(RutasSoap.ServicioLogin));

    [Fact]
    public void TheAppReadsTheServerFromAppsettingsJson()
    {
        using var app = new WebApplicationFactory<Program>();
        using IServiceScope alcance = app.Services.CreateScope();

        Assert.Equal("http://localhost:5171/LoginWebService.svc", alcance.ServiceProvider.GetRequiredService<LoginService>().Url);
        Assert.Equal("http://localhost:5171/MovimientoWebService.svc", alcance.ServiceProvider.GetRequiredService<MovimientoService>().Url);
    }

    [Fact]
    public void ChangingHostAndPuertoMovesEveryService()
    {
        using var app = new AppPrueba();
        using IServiceScope alcance = app.Services.CreateScope();

        Assert.Equal($"http://{AppPrueba.Host}:{AppPrueba.Puerto}/MovimientoWebService.svc",
            alcance.ServiceProvider.GetRequiredService<MovimientoService>().Url);
    }
}
