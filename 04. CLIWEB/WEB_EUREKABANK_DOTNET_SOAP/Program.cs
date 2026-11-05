using ec.edu.monster.config;
using ec.edu.monster.modelos.servicios;
using ec.edu.monster.modelos.soap;

var builder = WebApplication.CreateBuilder(args);

// Single configuration file: Servidor:Host and Servidor:Puerto in appsettings.json
builder.Services.Configure<ServidorOptions>(builder.Configuration.GetSection(ServidorOptions.Seccion));

// Model: SOAP transport (15 s timeout) and the services the controllers use
builder.Services.AddHttpClient<ITransporte, TransporteSoap>(cliente => cliente.Timeout = RutasSoap.TiempoEspera);
builder.Services.AddTransient<LoginService>();
builder.Services.AddTransient<MovimientoService>();

var app = builder.Build();

app.Run();

// Lets the tests start the app with WebApplicationFactory<Program>
public partial class Program;
