using Microsoft.Maui.Controls.Shapes;
using Path = Microsoft.Maui.Controls.Shapes.Path;

namespace MOV_EUREKABANK_DOTNET_SOAP.Vista.Componentes;

/// <summary>Ícono vectorial de 24x24 (ver <see cref="Iconos"/>), escalado al tamaño pedido sin deformarse.</summary>
public class Icono : ContentView
{
    private const double Lado = 24;

    public static readonly BindableProperty DatosProperty =
        BindableProperty.Create(nameof(Datos), typeof(string), typeof(Icono), null, propertyChanged: Redibujar);

    public static readonly BindableProperty ColorProperty =
        BindableProperty.Create(nameof(Color), typeof(Color), typeof(Icono), Colors.Black, propertyChanged: Redibujar);

    public static readonly BindableProperty TamanoProperty =
        BindableProperty.Create(nameof(Tamano), typeof(double), typeof(Icono), 24d, propertyChanged: Redibujar);

    private readonly Path _trazo = new() { Aspect = Stretch.None };

    public Icono()
    {
        InputTransparent = true;
        HorizontalOptions = LayoutOptions.Center;
        VerticalOptions = LayoutOptions.Center;
        Content = _trazo;
        Actualizar();
    }

    public string? Datos
    {
        get => (string?)GetValue(DatosProperty);
        set => SetValue(DatosProperty, value);
    }

    public Color Color
    {
        get => (Color)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public double Tamano
    {
        get => (double)GetValue(TamanoProperty);
        set => SetValue(TamanoProperty, value);
    }

    private static void Redibujar(BindableObject objeto, object anterior, object nuevo) => ((Icono)objeto).Actualizar();

    private void Actualizar()
    {
        _trazo.Data = string.IsNullOrEmpty(Datos)
            ? null
            : (Geometry?)new PathGeometryConverter().ConvertFromInvariantString(Datos);
        _trazo.Fill = new SolidColorBrush(Color);
        _trazo.StrokeThickness = 0;
        _trazo.RenderTransform = new ScaleTransform(Tamano / Lado, Tamano / Lado);
        _trazo.WidthRequest = _trazo.HeightRequest = Tamano;
        WidthRequest = HeightRequest = Tamano;
    }
}
