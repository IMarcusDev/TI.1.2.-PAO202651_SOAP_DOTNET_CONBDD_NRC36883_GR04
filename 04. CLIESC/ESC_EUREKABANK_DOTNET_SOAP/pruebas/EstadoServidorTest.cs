using System.Net;
using System.Net.Http;
using ec.edu.monster.config;
using ec.edu.monster.modelo;
using ec.edu.monster.pruebas.fakes;

namespace ec.edu.monster.pruebas;

public class EstadoServidorTest
{
    [Fact]
    public void ChecksTheWsdlOfTheLoginService()
        => Assert.Equal("http://localhost:5171/LoginWebService.svc?wsdl", Crear(_ => new HttpResponseMessage(HttpStatusCode.OK)).Url);

    [Fact]
    public async Task AnAnswerBelow500MeansTheServerIsRunning()
    {
        Assert.True(await Crear(_ => new HttpResponseMessage(HttpStatusCode.OK)).EstaDisponibleAsync());
        Assert.True(await Crear(_ => new HttpResponseMessage(HttpStatusCode.NotFound)).EstaDisponibleAsync());
    }

    [Fact]
    public async Task A500OrAConnectionFailureMeansTheServerIsNotRunning()
    {
        Assert.False(await Crear(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError)).EstaDisponibleAsync());
        Assert.False(await Crear(_ => throw new HttpRequestException("caído")).EstaDisponibleAsync());
    }

    private static EstadoServidor Crear(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        var handler = new FakeHttpHandler(responder);
        return new EstadoServidor(new Configuracion(), new HttpClient(handler) { Timeout = RutasSoap.TiempoEsperaEstado });
    }
}
