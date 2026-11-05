using System.Text.Encodings.Web;
using System.Text.Unicode;
using ec.edu.monster.config;
using ec.edu.monster.modelos.servicios;
using ec.edu.monster.modelos.soap;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.WebEncoders;

var builder = WebApplication.CreateBuilder(args);

// Single configuration file: Servidor:Host and Servidor:Puerto in appsettings.json
builder.Services.Configure<ServidorOptions>(builder.Configuration.GetSection(ServidorOptions.Seccion));

// Model: SOAP transport (15 s timeout) and the services the controllers use
builder.Services.AddHttpClient<ITransporte, TransporteSoap>(cliente => cliente.Timeout = RutasSoap.TiempoEspera);
builder.Services.AddTransient<LoginService>();
builder.Services.AddTransient<MovimientoService>();

// Every POST must carry the antiforgery token (replaces the CsrfFilter of the Java client)
builder.Services.AddControllersWithViews(opciones =>
    opciones.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

// Views live in ec/edu/monster/vista, like the JSPs of the Java clients
builder.Services.Configure<RazorViewEngineOptions>(opciones =>
{
    opciones.ViewLocationFormats.Clear();
    opciones.ViewLocationFormats.Add("/ec/edu/monster/vista/{0}.cshtml");
    opciones.ViewLocationFormats.Add("/ec/edu/monster/vista/partials/{0}.cshtml");
});

// Spanish text (á, ñ, N.º) is written as is instead of &#xE1; entities; HTML characters are still escaped
builder.Services.Configure<WebEncoderOptions>(opciones =>
    opciones.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));

// Server-side session (cookie only carries the id)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(opciones =>
{
    opciones.Cookie.Name = ".EurekaBank.Sesion";
    opciones.Cookie.HttpOnly = true;
    opciones.Cookie.IsEssential = true;
    opciones.Cookie.SameSite = SameSiteMode.Strict;
    opciones.IdleTimeout = TimeSpan.FromMinutes(30);
});

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseSession();

// The application starts at the login screen
app.MapGet("/", (HttpContext contexto) => Results.Redirect($"{contexto.Request.PathBase}/login"));
app.MapControllers();

app.Run();

// Lets the tests start the app with WebApplicationFactory<Program>
public partial class Program;
