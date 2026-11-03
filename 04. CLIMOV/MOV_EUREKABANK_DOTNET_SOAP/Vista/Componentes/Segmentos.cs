using Microsoft.Maui.Controls.Shapes;

namespace MOV_EUREKABANK_DOTNET_SOAP.Vista.Componentes;

/// <summary>Grupo de botones de selección única (MaterialButtonToggleGroup del cliente Android).</summary>
public class Segmentos : ContentView
{
    public record Opcion(object Clave, string Texto, string Icono);

    private readonly Grid _grupo = new() { ColumnSpacing = 1, BackgroundColor = Colors.Transparent };
    private readonly List<(Opcion Opcion, Grid Vista, Icono Icono, Label Texto)> _segmentos = [];
    private object? _seleccion;

    public Segmentos()
    {
        _grupo.BackgroundColor = Colores.BordeCampo;
        Content = new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = 10 },
            Stroke = Colores.BordeCampo,
            StrokeThickness = 1,
            Padding = 0,
            HeightRequest = 48,
            Content = _grupo,
        };
    }

    /// <summary>Se dispara solo cuando el usuario toca un segmento distinto del elegido.</summary>
    public event EventHandler<object>? SeleccionCambiada;

    public object? Seleccion
    {
        get => _seleccion;
        set
        {
            _seleccion = value;
            Pintar();
        }
    }

    public void Configurar(IEnumerable<Opcion> opciones)
    {
        _grupo.Children.Clear();
        _grupo.ColumnDefinitions.Clear();
        _segmentos.Clear();

        foreach (Opcion opcion in opciones)
        {
            var icono = new Icono { Datos = opcion.Icono, Tamano = 18 };
            var texto = new Label { Text = opcion.Texto, FontSize = 14, FontFamily = "OpenSansSemibold", VerticalOptions = LayoutOptions.Center, LineBreakMode = LineBreakMode.TailTruncation };
            var vista = new Grid
            {
                Padding = new Thickness(4, 0),
                Children =
                {
                    new HorizontalStackLayout
                    {
                        Spacing = 6,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        Children = { icono, texto },
                    },
                },
            };
            vista.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => AlTocar(opcion.Clave)) });
            SemanticProperties.SetDescription(vista, opcion.Texto);

            _grupo.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            _grupo.Add(vista, _segmentos.Count);
            _segmentos.Add((opcion, vista, icono, texto));
        }
        Pintar();
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == nameof(IsEnabled))
            Opacity = IsEnabled ? 1 : 0.6;
    }

    private void AlTocar(object clave)
    {
        if (!IsEnabled || Equals(clave, _seleccion))
            return;
        Seleccion = clave;
        SeleccionCambiada?.Invoke(this, clave);
    }

    private void Pintar()
    {
        foreach (var (opcion, vista, icono, texto) in _segmentos)
        {
            bool elegido = Equals(opcion.Clave, _seleccion);
            vista.BackgroundColor = elegido ? Colores.PrimarioSuave : Colors.White;
            Color color = elegido ? Colores.Primario : Colores.TextoOscuro;
            icono.Color = color;
            texto.TextColor = color;
        }
    }
}
