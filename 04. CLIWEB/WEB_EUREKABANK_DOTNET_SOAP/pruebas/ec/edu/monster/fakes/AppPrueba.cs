using System.Net;
using System.Text.RegularExpressions;
using ec.edu.monster.modelos.soap;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace ec.edu.monster.fakes;

/// <summary>
/// The real web app (Program) with the SOAP transport replaced by FakeTransporte and the server
/// address pointed at a test host, so tests can check the URL the services build.
/// </summary>
public class AppPrueba : WebApplicationFactory<Program>
{
    public const string Host = "servidor-prueba";
    public const int Puerto = 5999;

    public FakeTransporte Transporte { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Servidor:Host", Host);
        builder.UseSetting("Servidor:Puerto", Puerto.ToString());
        builder.ConfigureServices(servicios => servicios.AddSingleton<ITransporte>(Transporte));
    }

    /// <summary>A browser with its own cookies that does not follow redirects.</summary>
    public Navegador Navegador() => new(CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        HandleCookies = true,
    }), Transporte);
}

/// <summary>Browser helper: reads the antiforgery token of a page and posts forms with it.</summary>
public partial class Navegador(HttpClient cliente, FakeTransporte transporte)
{
    public HttpClient Cliente { get; } = cliente;

    public Task<HttpResponseMessage> GetAsync(string ruta) => Cliente.GetAsync(ruta);

    public async Task<string> HtmlAsync(string ruta)
    {
        HttpResponseMessage respuesta = await Cliente.GetAsync(ruta);
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        return await respuesta.Content.ReadAsStringAsync();
    }

    /// <summary>Posts a form like the browser does: the token comes from the page that shows the form.</summary>
    public async Task<HttpResponseMessage> PostAsync(string ruta, params (string Nombre, string Valor)[] campos)
    {
        string pagina = await HtmlAsync(ruta == "/logout" ? "/menu" : ruta);
        string token = TokenDe(pagina);
        var formulario = new List<KeyValuePair<string, string>> { new("__RequestVerificationToken", token) };
        formulario.AddRange(campos.Select(c => new KeyValuePair<string, string>(c.Nombre, c.Valor)));
        return await Cliente.PostAsync(ruta, new FormUrlEncodedContent(formulario));
    }

    public async Task<string> PostHtmlAsync(string ruta, params (string Nombre, string Valor)[] campos)
    {
        HttpResponseMessage respuesta = await PostAsync(ruta, campos);
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        return await respuesta.Content.ReadAsStringAsync();
    }

    public async Task LoginEmpleadoAsync(string nombre = "Monster Bank")
    {
        transporte.Responder("loginEmpleado", SoapRespuestas.LoginOk("loginEmpleado", nombre, "Empleado"));
        HttpResponseMessage respuesta = await PostAsync("/login",
            ("tipo", "empleado"), ("username", "monster"), ("password", "monster9"));
        Assert.Equal("/menu", respuesta.Headers.Location?.OriginalString);
    }

    public async Task LoginClienteAsync(string cuenta = "00100001", string nombre = "Ana Pérez")
    {
        transporte.Responder("loginCliente", SoapRespuestas.LoginOk("loginCliente", nombre, "Cliente"));
        HttpResponseMessage respuesta = await PostAsync("/login",
            ("tipo", "cliente"), ("username", cuenta), ("password", "123456"));
        Assert.Equal("/menu", respuesta.Headers.Location?.OriginalString);
    }

    public static string TokenDe(string html)
    {
        Match coincidencia = PatronToken().Match(html);
        Assert.True(coincidencia.Success, "The page has no antiforgery token");
        return WebUtility.HtmlDecode(coincidencia.Groups[1].Value);
    }

    [GeneratedRegex("name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"")]
    private static partial Regex PatronToken();
}
