using Microsoft.Maui.Controls.Shapes;
using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Data;
using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Enums;
using MOV_EUREKABANK_DOTNET_SOAP.Utils;
using MOV_EUREKABANK_DOTNET_SOAP.Vista.Componentes;

namespace MOV_EUREKABANK_DOTNET_SOAP.Vista;

/// <summary>
/// ECUD3 paso 14: comprobante con el tipo, las cuentas, el importe, el N.º de movimiento, el nuevo saldo
/// y la fecha. Se muestra como una hoja inferior sobre la pantalla de operaciones.
/// </summary>
public class ComprobantePage : ContentPage
{
    public ComprobantePage(string? mensaje, Comprobante comprobante, string? moneda)
    {
        SolicitudOperacion solicitud = comprobante.Solicitud;
        bool transferencia = solicitud.Tipo == TipoOperacion.Transferencia;
        BackgroundColor = Color.FromArgb("#66000000");
        Shell.SetPresentationMode(this, PresentationMode.ModalAnimated);

        var datos = new VerticalStackLayout();
        var filas = new List<View>
        {
            Filas.Dato(Textos.CompOperacion, Textos.NombreOperacion(solicitud.Tipo)),
            Filas.Dato(transferencia ? Textos.CompCuentaOrigen : Textos.CompCuenta, solicitud.Cuenta),
        };
        if (transferencia)
            filas.Add(Filas.Dato(Textos.CompCuentaDestino, solicitud.CuentaDestino ?? Formato.SinDato));
        filas.Add(Filas.Dato(Textos.CompNumero, string.IsNullOrEmpty(comprobante.NumeroMovimiento) ? Formato.SinDato : comprobante.NumeroMovimiento));
        filas.Add(Filas.Dato(Textos.CompNuevoSaldo, Formato.Importe(comprobante.NuevoSaldo, moneda)));
        filas.Add(Filas.Dato(Textos.CompFecha, Formato.Fecha(comprobante.Fecha)));
        Filas.Llenar(datos, filas);

        var listo = new Button { Text = "Listo", Margin = new Thickness(0, 20, 0, 0) };
        if (Application.Current?.Resources.TryGetValue("BotonPrimario", out object? estilo) == true)
            listo.Style = (Style)estilo;
        listo.Clicked += async (_, _) => await CerrarAsync();

        var hoja = new Border
        {
            BackgroundColor = Colors.White,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(24, 24, 0, 0) },
            Padding = new Thickness(24, 12, 24, 24),
            VerticalOptions = LayoutOptions.End,
            Content = new VerticalStackLayout
            {
                Spacing = 0,
                Children =
                {
                    new BoxView { WidthRequest = 36, HeightRequest = 4, CornerRadius = 2, Color = Colores.BordeTarjeta, HorizontalOptions = LayoutOptions.Center },
                    new Border
                    {
                        Margin = new Thickness(0, 20, 0, 0),
                        WidthRequest = 64,
                        HeightRequest = 64,
                        StrokeThickness = 0,
                        StrokeShape = new Ellipse(),
                        BackgroundColor = Colores.IngresoFondo,
                        HorizontalOptions = LayoutOptions.Center,
                        Content = new Icono { Datos = Iconos.Exito, Color = Colores.Ingreso, Tamano = 36 },
                    },
                    new Label
                    {
                        Text = string.IsNullOrEmpty(mensaje) ? Textos.NombreOperacion(solicitud.Tipo) : mensaje,
                        FontSize = 16,
                        TextColor = Colores.TextoSecundario,
                        HorizontalTextAlignment = TextAlignment.Center,
                        Margin = new Thickness(0, 12, 0, 0),
                    },
                    new Label
                    {
                        Text = Formato.Importe(solicitud.Importe, moneda),
                        FontSize = 30,
                        FontFamily = "OpenSansSemibold",
                        TextColor = Colores.TextoOscuro,
                        HorizontalTextAlignment = TextAlignment.Center,
                        Margin = new Thickness(0, 4, 0, 0),
                    },
                    new Label
                    {
                        Text = "Comprobante",
                        FontSize = 12,
                        CharacterSpacing = 0.7,
                        TextTransform = TextTransform.Uppercase,
                        TextColor = Colores.TextoSecundario,
                        Margin = new Thickness(0, 20, 0, 8),
                    },
                    new Border
                    {
                        BackgroundColor = Colors.White,
                        Stroke = Colores.BordeTarjeta,
                        StrokeThickness = 1,
                        StrokeShape = new RoundRectangle { CornerRadius = 16 },
                        Content = datos,
                    },
                    listo,
                },
            },
        };

        var fondo = new Grid();
        var cerrarAlTocarFuera = new BoxView { Color = Colors.Transparent };
        cerrarAlTocarFuera.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(async () => await CerrarAsync()) });
        fondo.Add(cerrarAlTocarFuera);
        fondo.Add(hoja);
        Content = fondo;
    }

    protected override bool OnBackButtonPressed()
    {
        _ = CerrarAsync();
        return true;
    }

    private async Task CerrarAsync()
    {
        if (Navigation.ModalStack.Contains(this))
            await Navigation.PopModalAsync();
    }
}
