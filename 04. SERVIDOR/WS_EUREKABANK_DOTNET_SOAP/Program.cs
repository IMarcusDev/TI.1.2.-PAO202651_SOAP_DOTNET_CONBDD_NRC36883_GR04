using SoapCore;
using WS_CON_UNI_DOTNET_SOAP.WS.Interfaces;
using WS_CON_UNI_DOTNET_SOAP.WS.Services;

var builder = WebApplication.CreateBuilder(args);

// Registrar SoapCore
builder.Services.AddSoapCore();

// Registrar servicios SOAP
builder.Services.AddSingleton<ILoginWebService, LoginWebService>();
builder.Services.AddSingleton<IMovimientoWebService, MovimientoWebService>();

// Controllers y OpenAPI
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configuración del pipeline
if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// Endpoints SOAP
app.UseRouting();

app.UseEndpoints(endpoints =>
{
  endpoints.UseSoapEndpoint<ILoginWebService>(
    "/LoginWebService.svc",
    new SoapEncoderOptions(),
    SoapSerializer.XmlSerializer
  );

  endpoints.UseSoapEndpoint<IMovimientoWebService>(
    "/MovimientoWebService.svc",
    new SoapEncoderOptions(),
    SoapSerializer.XmlSerializer
  );
});

app.MapControllers();

app.Run();