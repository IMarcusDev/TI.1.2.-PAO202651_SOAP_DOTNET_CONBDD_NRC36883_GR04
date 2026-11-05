using System.Net;
using ec.edu.monster.fakes;

namespace ec.edu.monster.controladores;

/// <summary>Login, logout, menu and the access filter, through HTTP like a browser.</summary>
public class SesionTest : IDisposable
{
    private readonly AppPrueba _app = new();

    public void Dispose() => _app.Dispose();

    [Fact]
    public async Task TheRootRedirectsToTheLogin()
    {
        HttpResponseMessage respuesta = await _app.Navegador().GetAsync("/");

        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.Equal("/login", respuesta.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task TheLoginShowsBothRolesAndTheSoapDotNetBadge()
    {
        string html = await _app.Navegador().HtmlAsync("/login");

        Assert.Contains("value=\"empleado\" checked=\"checked\"", html);
        Assert.Contains("value=\"cliente\"", html);
        Assert.Contains("<span class=\"badge__protocol--soap\">SOAP</span>", html);
        Assert.Contains("<span class=\"badge__platform--dotnet\">.NET</span>", html);
        Assert.Contains("/css/eurekabank.css", html);
    }

    [Theory]
    [InlineData("empleado", "Ingrese usuario y contraseña")]
    [InlineData("cliente", "Ingrese número de cuenta y clave")]
    public async Task AnIncompleteFormIsRejectedWithoutCallingTheServer(string tipo, string mensaje)
    {
        string html = await _app.Navegador().PostHtmlAsync("/login", ("tipo", tipo), ("username", "abc"), ("password", " "));

        Assert.Contains(mensaje, html);
        Assert.Contains("value=\"abc\"", html);
        Assert.Empty(_app.Transporte.Peticiones);
    }

    [Fact]
    public async Task AnEmpleadoLogsInAndSeesTheEmployeeMenu()
    {
        Navegador navegador = _app.Navegador();
        await navegador.LoginEmpleadoAsync("Monster Bank");

        string html = await navegador.HtmlAsync("/menu");

        Assert.Equal("monster", _app.Transporte.Ultima("loginEmpleado").Parametro("usuario"));
        Assert.Contains("Bienvenido, Monster Bank", html);
        Assert.Contains("<span class=\"user__role\">Empleado</span>", html);
        Assert.Contains("href=\"/deposito\"", html);
        Assert.Contains("Este cliente consume el servicio SOAP desarrollado en .NET.", html);
    }

    [Fact]
    public async Task AClienteLogsInWithTheAccountAndSeesDepositoDisabled()
    {
        Navegador navegador = _app.Navegador();
        await navegador.LoginClienteAsync("00100001");

        string html = await navegador.HtmlAsync("/menu");

        Assert.Equal("00100001", _app.Transporte.Ultima("loginCliente").Parametro("cuenta"));
        Assert.DoesNotContain("href=\"/deposito\"", html);
        Assert.Contains("card card--disabled", html);
        Assert.Contains("nav__item nav__item--disabled", html);
    }

    [Fact]
    public async Task AWrongLoginShowsTheServerMessageAndKeepsTheRoleAndUser()
    {
        _app.Transporte.Responder("loginCliente", SoapRespuestas.Error("loginCliente", "Cuenta o clave incorrecta"));

        string html = await _app.Navegador().PostHtmlAsync("/login", ("tipo", "cliente"), ("username", "00100001"), ("password", "x"));

        Assert.Contains("Cuenta o clave incorrecta", html);
        Assert.Contains("value=\"cliente\" checked=\"checked\"", html);
        Assert.Contains("value=\"00100001\"", html);
    }

    [Fact]
    public async Task TheExpiredNoticeIsShownAfterTheServerEndsTheSession()
    {
        string html = await _app.Navegador().HtmlAsync("/login?expirada=1");

        Assert.Contains(LoginController.AvisoExpirada, html);
    }

    [Fact]
    public async Task APostWithoutTheAntiforgeryTokenIsRejected()
    {
        HttpResponseMessage respuesta = await _app.Navegador().Cliente.PostAsync("/login",
            new FormUrlEncodedContent([new("username", "monster"), new("password", "monster9")]));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Empty(_app.Transporte.Peticiones);
    }

    [Theory]
    [InlineData("/menu")]
    [InlineData("/deposito")]
    [InlineData("/retiro")]
    [InlineData("/transferencia")]
    public async Task ProtectedScreensSendAVisitorToTheLogin(string ruta)
    {
        HttpResponseMessage respuesta = await _app.Navegador().GetAsync(ruta);

        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.Equal("/login", respuesta.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task ProtectedScreensAreNotCached()
    {
        Navegador navegador = _app.Navegador();
        await navegador.LoginEmpleadoAsync();

        HttpResponseMessage respuesta = await navegador.GetAsync("/menu");

        Assert.Contains("no-store", respuesta.Headers.CacheControl?.ToString());
    }

    [Fact]
    public async Task LogoutEndsTheTokenOnTheServerAndTheLocalSession()
    {
        Navegador navegador = _app.Navegador();
        await navegador.LoginEmpleadoAsync();
        _app.Transporte.Responder("logout", SoapRespuestas.Sobre("logout", "<Estado>OK</Estado>"));

        HttpResponseMessage salida = await navegador.PostAsync("/logout");
        HttpResponseMessage menu = await navegador.GetAsync("/menu");

        Assert.Equal("/login", salida.Headers.Location?.OriginalString);
        Assert.Equal("token-123", _app.Transporte.Ultima("logout").Parametro("token"));
        Assert.Equal("/login", menu.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task LogoutStillEndsTheLocalSessionWhenTheServerIsDown()
    {
        Navegador navegador = _app.Navegador();
        await navegador.LoginEmpleadoAsync();

        await navegador.PostAsync("/logout");
        HttpResponseMessage menu = await navegador.GetAsync("/menu");

        Assert.Equal("/login", menu.Headers.Location?.OriginalString);
    }
}
