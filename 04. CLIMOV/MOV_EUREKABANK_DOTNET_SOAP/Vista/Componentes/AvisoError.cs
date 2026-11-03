using Microsoft.Maui.Controls.Shapes;

namespace MOV_EUREKABANK_DOTNET_SOAP.Vista.Componentes;

/// <summary>Mensaje de error destacado con fondo rojo claro; se oculta cuando no hay texto.</summary>
public class AvisoError : ContentView
{
    private readonly Label _texto = new() { FontSize = 14, VerticalOptions = LayoutOptions.Center };

    public AvisoError()
    {
        _texto.TextColor = Colores.TextoError;
        var fila = new Grid
        {
            ColumnSpacing = 10,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) },
        };
        fila.Add(new Icono { Datos = Iconos.Error, Tamano = 20, Color = Colores.TextoError, VerticalOptions = LayoutOptions.Start }, 0);
        fila.Add(_texto, 1);

        Content = new Border
        {
            BackgroundColor = Colores.ErrorFondo,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 10 },
            Padding = 12,
            Content = fila,
        };
        IsVisible = false;
        SemanticProperties.SetDescription(this, "");
    }

    public string? Texto
    {
        get => _texto.Text;
        set
        {
            _texto.Text = value;
            IsVisible = !string.IsNullOrEmpty(value);
            SemanticProperties.SetDescription(this, value ?? "");
            if (IsVisible)
                SemanticScreenReader.Announce(value!);
        }
    }
}
