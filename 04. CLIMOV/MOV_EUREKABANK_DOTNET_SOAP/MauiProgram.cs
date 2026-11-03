using Microsoft.Extensions.Logging;
using MOV_EUREKABANK_DOTNET_SOAP.Config;
using MOV_EUREKABANK_DOTNET_SOAP.Controlador;
using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Interfaces;
using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Repositorios;
using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Soap;
using MOV_EUREKABANK_DOTNET_SOAP.Servicios;
using MOV_EUREKABANK_DOTNET_SOAP.Vista;

namespace MOV_EUREKABANK_DOTNET_SOAP
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Modelo: la sesión y los repositorios SOAP viven mientras dura la app.
            builder.Services.AddSingleton<SesionServicio>();
            builder.Services.AddSingleton<IVerificadorRed, RedMaui>();
            builder.Services.AddSingleton<SoapTransporte>();
            builder.Services.AddSingleton<ILoginRepositorio>(s =>
                new LoginSoapRepositorio(SoapConfig.UrlBase, s.GetRequiredService<SoapTransporte>(), s.GetRequiredService<IVerificadorRed>()));
            builder.Services.AddSingleton<IMovimientoRepositorio>(s =>
                new MovimientoSoapRepositorio(SoapConfig.UrlBase, s.GetRequiredService<SoapTransporte>(), s.GetRequiredService<IVerificadorRed>()));

            // Controladores y vistas
            builder.Services.AddTransient<LoginControlador>();
            builder.Services.AddTransient<MovimientosControlador>();
            builder.Services.AddTransient<OperacionControlador>();

            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<InicioPage>();
            builder.Services.AddTransient<MovimientosPage>();
            builder.Services.AddTransient<OperacionesPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // Sin la línea inferior que Android dibuja en los Entry: el contorno lo pone CampoTexto.
            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("SinSubrayado", (handler, view) =>
            {
#if ANDROID
                handler.PlatformView.BackgroundTintList =
                    Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#endif
            });

            return builder.Build();
        }
    }
}
