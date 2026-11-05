using System.Net;
using System.Net.Http;
using ec.edu.monster.config;
using ec.edu.monster.modelo;
using ec.edu.monster.pruebas.fakes;
using ec.edu.monster.vista.modelos;

namespace ec.edu.monster.pruebas;

/// <summary>
/// Server status indicator: it starts in "checking", turns green when the WSDL answers and red
/// when the server fails, and notifies the view when the state changes.
/// </summary>
public class EstadoServidorViewModelTest
{
    private static (EstadoServidorViewModel Modelo, FakeHttpHandler Handler) Crear(HttpStatusCode codigo)
    {
        var handler = new FakeHttpHandler(_ => new HttpResponseMessage(codigo));
        var estado = new EstadoServidor(new Configuracion(), new HttpClient(handler));
        return (new EstadoServidorViewModel(estado), handler);
    }

    [Fact]
    public void Empieza_comprobando()
    {
        var (modelo, _) = Crear(HttpStatusCode.OK);

        Assert.Equal(EstadoServidorViewModel.EstadoComprobando, modelo.Estado);
        Assert.Equal(EstadoServidorViewModel.ComprobandoTexto, modelo.Texto);
        Assert.False(modelo.EstaDisponible);
    }

    [Fact]
    public async Task Servidor_que_responde_se_muestra_disponible()
    {
        var (modelo, handler) = Crear(HttpStatusCode.OK);

        await modelo.RevisarAsync();

        Assert.Equal(EstadoServidorViewModel.EstadoDisponible, modelo.Estado);
        Assert.Equal("Servicio disponible", modelo.Texto);
        Assert.True(modelo.EstaDisponible);
        Assert.Contains("LoginWebService.svc?wsdl", Assert.Single(handler.Urls));
    }

    [Fact]
    public async Task Servidor_con_error_se_muestra_no_disponible()
    {
        var (modelo, _) = Crear(HttpStatusCode.InternalServerError);

        await modelo.RevisarAsync();

        Assert.Equal(EstadoServidorViewModel.EstadoNoDisponible, modelo.Estado);
        Assert.Equal("Servicio no disponible", modelo.Texto);
        Assert.False(modelo.EstaDisponible);
    }

    [Fact]
    public async Task Avisa_al_cambiar_el_estado()
    {
        var (modelo, _) = Crear(HttpStatusCode.OK);
        var cambiadas = new List<string?>();
        modelo.PropertyChanged += (_, e) => cambiadas.Add(e.PropertyName);

        await modelo.RevisarAsync();

        Assert.Contains(nameof(EstadoServidorViewModel.Estado), cambiadas);
        Assert.Contains(nameof(EstadoServidorViewModel.Texto), cambiadas);
        Assert.Contains(nameof(EstadoServidorViewModel.EstaDisponible), cambiadas);
    }

    [Fact]
    public async Task Detener_corta_el_ciclo_de_revisiones()
    {
        var (modelo, _) = Crear(HttpStatusCode.OK);

        Task ciclo = modelo.IniciarAsync();
        modelo.Detener();
        Task terminado = await Task.WhenAny(ciclo, Task.Delay(TimeSpan.FromSeconds(2)));

        Assert.Same(ciclo, terminado);
    }
}
