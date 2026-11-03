using MOV_EUREKABANK_DOTNET_SOAP.Controlador;
using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Data;
using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Enums;

namespace MOV_EUREKABANK_DOTNET_SOAP.Vista;

/// <summary>
/// Pestaña de la pantalla principal (ERS 8.1.1): barra con el título, el nombre y el perfil, y la opción
/// Cerrar sesión. Verifica la sesión cada vez que se muestra.
/// </summary>
public abstract class PestanaPage : ContentPage
{
    private readonly LoginControlador _login;
    private readonly Label _subtitulo = new() { FontSize = 13, LineBreakMode = LineBreakMode.TailTruncation };

    protected PestanaPage(LoginControlador login, string titulo)
    {
        _login = login;
        Title = titulo;

        if (Application.Current?.Resources.TryGetValue("TextoSobrePrimario", out object? color) == true)
            _subtitulo.TextColor = (Color)color;
        Shell.SetTitleView(this, new VerticalStackLayout
        {
            VerticalOptions = LayoutOptions.Center,
            Spacing = 0,
            Children =
            {
                new Label { Text = titulo, TextColor = Colors.White, FontSize = 20, FontFamily = "OpenSansSemibold" },
                _subtitulo,
            },
        });

        ToolbarItems.Add(new ToolbarItem
        {
            Text = Textos.CerrarSesion,
            IconImageSource = "ic_salir.png",
            Order = ToolbarItemOrder.Primary,
            Command = new Command(async () => await ConfirmarCierreSesionAsync()),
        });
    }

    protected abstract ControladorConSesion Controlador { get; }

    /// <summary>Se llama cada vez que la pestaña se vuelve visible con una sesión vigente.</summary>
    protected abstract void AlMostrar();

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (!Controlador.HaySesion)
        {
            _ = VolverAlLoginAsync(ControladorConSesion.MensajeSesionExpirada);
            return;
        }
        _subtitulo.Text = Subtitulo(Controlador.Sesion!);
        AlMostrar();
    }

    protected Task VolverAlLoginAsync(string? aviso) => NavegacionSesion.VolverAlLoginAsync(this, aviso);

    private static string Subtitulo(Sesion sesion)
    {
        string perfil = sesion.Perfil.Etiqueta();
        return string.IsNullOrEmpty(sesion.Nombre) ? perfil : $"{sesion.Nombre} · {perfil}";
    }

    private async Task ConfirmarCierreSesionAsync()
    {
        if (!await DisplayAlertAsync(Textos.CerrarSesion, Textos.CerrarSesionPregunta, Textos.Si, Textos.No))
            return;

        // ECUD1 FA5: la sesión local se borra de inmediato y el logout se avisa en segundo plano.
        string? token = _login.CerrarSesion();
        _ = _login.NotificarCierreAsync(token);
        await VolverAlLoginAsync(null);
    }
}
