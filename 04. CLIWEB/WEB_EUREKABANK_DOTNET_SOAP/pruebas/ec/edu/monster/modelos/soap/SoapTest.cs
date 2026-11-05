using System.Net;
using ec.edu.monster.fakes;
using ec.edu.monster.modelos.soap;

namespace ec.edu.monster.modelos.soap;

public class SoapConstructorPeticionTest
{
    [Fact]
    public void BuildsAWrappedEnvelopeWithEscapedValuesAndAmountWithTwoDecimals()
    {
        string sobre = new SoapConstructorPeticion("http://tempuri.org/", "registrarRetiro")
            .Parametro("cuenta", "001<&>")
            .Parametro("importe", 120.5m)
            .Parametro("omitido", (string?)null)
            .Construir();

        Assert.Equal(
            "<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\"><soap:Body>"
            + "<registrarRetiro xmlns=\"http://tempuri.org/\"><cuenta>001&lt;&amp;&gt;</cuenta><importe>120.50</importe></registrarRetiro>"
            + "</soap:Body></soap:Envelope>",
            sobre);
    }
}

public class SoapLectorRespuestaTest
{
    [Fact]
    public void ReadsPascalCaseFieldsIgnoringCaseAndNesting()
    {
        var lector = new SoapLectorRespuesta(SoapRespuestas.LoginOk("loginEmpleado", "Monster", "Empleado"));

        Assert.True(lector.EsOk());
        Assert.Equal("token-123", lector.Texto("token"));
        Assert.Equal("Empleado", lector.Texto("tipoUsuario"));
        Assert.Null(lector.Texto("noExiste"));
    }

    [Fact]
    public void ReadsTheMovementListInsideItsContainer()
    {
        var lector = new SoapLectorRespuesta(SoapRespuestas.ConsultaOk(
            (1, "2026-10-30T09:15:00", "Depósito", "INGRESO", "500.00", ""),
            (2, "2026-10-31T10:00:00", "Retiro", "SALIDA", "120.50", "")));

        IReadOnlyList<IReadOnlyDictionary<string, string>> registros = lector.Registros("numeroMovimiento");

        Assert.Equal(2, registros.Count);
        Assert.Equal("120.50", registros[1]["importe"]);
    }

    [Fact]
    public void ThrowsOnASoapFault()
        => Assert.Throws<SoapFallaException>(() => new SoapLectorRespuesta(SoapRespuestas.Falla("boom")));

    [Theory]
    [InlineData("")]
    [InlineData("no es xml")]
    public void ThrowsOnAnEmptyOrInvalidResponse(string xml)
        => Assert.Throws<RespuestaInvalidaException>(() => new SoapLectorRespuesta(xml));
}

public class TransporteSoapTest
{
    [Fact]
    public async Task PostsTheEnvelopeWithTheQuotedSoapActionAsTextXml()
    {
        var manejador = new ManejadorFalso(HttpStatusCode.OK, "<ok/>");
        var transporte = new TransporteSoap(new HttpClient(manejador));

        string respuesta = await transporte.EnviarAsync("http://h:1/LoginWebService.svc", "http://tempuri.org/ILoginWebService/logout", "<sobre/>");

        Assert.Equal("<ok/>", respuesta);
        Assert.Equal(HttpMethod.Post, manejador.Metodo);
        Assert.Equal("http://h:1/LoginWebService.svc", manejador.Url);
        Assert.Equal("\"http://tempuri.org/ILoginWebService/logout\"", manejador.SoapAction);
        Assert.Equal("text/xml", manejador.TipoContenido);
        Assert.Equal("<sobre/>", manejador.Cuerpo);
    }

    [Fact]
    public async Task ReturnsTheBodyOfAnHttp500BecauseItCarriesTheFault()
    {
        var transporte = new TransporteSoap(new HttpClient(new ManejadorFalso(HttpStatusCode.InternalServerError, "<fault/>")));

        Assert.Equal("<fault/>", await transporte.EnviarAsync("http://h:1/x.svc", "a", "<s/>"));
    }

    [Fact]
    public async Task ThrowsOnAnyOtherHttpStatus()
    {
        var transporte = new TransporteSoap(new HttpClient(new ManejadorFalso(HttpStatusCode.NotFound, "")));

        HttpEstadoException error = await Assert.ThrowsAsync<HttpEstadoException>(
            () => transporte.EnviarAsync("http://h:1/x.svc", "a", "<s/>"));
        Assert.Equal(404, error.Codigo);
    }

    private sealed class ManejadorFalso(HttpStatusCode estado, string cuerpo) : HttpMessageHandler
    {
        public HttpMethod? Metodo { get; private set; }
        public string? Url { get; private set; }
        public string? SoapAction { get; private set; }
        public string? TipoContenido { get; private set; }
        public string? Cuerpo { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage peticion, CancellationToken cancelacion)
        {
            Metodo = peticion.Method;
            Url = peticion.RequestUri?.ToString();
            SoapAction = peticion.Headers.GetValues("SOAPAction").Single();
            TipoContenido = peticion.Content?.Headers.ContentType?.MediaType;
            Cuerpo = peticion.Content is null ? null : await peticion.Content.ReadAsStringAsync(cancelacion);
            return new HttpResponseMessage(estado) { Content = new StringContent(cuerpo) };
        }
    }
}
