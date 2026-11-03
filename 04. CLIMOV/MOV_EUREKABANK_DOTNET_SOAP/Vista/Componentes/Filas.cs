using Microsoft.Maui.Controls.Shapes;
using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Data;
using MOV_EUREKABANK_DOTNET_SOAP.Utils;

namespace MOV_EUREKABANK_DOTNET_SOAP.Vista.Componentes;

/// <summary>Filas que se agregan a las listas de las tarjetas (item_movimiento, item_accion e item_dato del cliente Android).</summary>
public static class Filas
{
    /// <summary>Fila de un movimiento: N.º, fecha, tipo, acción, importe y cuenta de referencia (ECUD2 paso 9).</summary>
    public static View Movimiento(Movimiento movimiento, string? moneda)
    {
        bool ingreso = movimiento.EsIngreso;
        Color color = ingreso ? Colores.Ingreso : Colores.Salida;

        var detalle = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
        detalle.Add(new Label
        {
            Text = movimiento.Tipo,
            FontSize = 15,
            FontFamily = "OpenSansSemibold",
            TextColor = Colores.TextoOscuro,
            LineBreakMode = LineBreakMode.TailTruncation,
            MaxLines = 1,
        });
        detalle.Add(Secundario(string.Format(Textos.MovNumeroFecha, movimiento.Numero, Formato.Fecha(movimiento.Fecha)), 13));
        if (movimiento.TieneReferencia)
            detalle.Add(Secundario(string.Format(Textos.MovReferencia, movimiento.CuentaReferencia), 12));

        var importe = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
        importe.Add(new Label
        {
            Text = (ingreso ? "+" : "-") + Formato.Importe(movimiento.Importe, moneda),
            FontSize = 15,
            FontFamily = "OpenSansSemibold",
            TextColor = color,
            HorizontalOptions = LayoutOptions.End,
        });
        importe.Add(new Label
        {
            Text = ingreso ? Textos.MovIngreso : Textos.MovSalida,
            FontSize = 11,
            CharacterSpacing = 0.7,
            TextTransform = TextTransform.Uppercase,
            TextColor = color,
            HorizontalOptions = LayoutOptions.End,
        });

        Grid fila = FilaBase(16);
        fila.Add(Circulo(ingreso ? Iconos.Ingreso : Iconos.Salida, color, ingreso ? Colores.IngresoFondo : Colores.SalidaFondo, 40, 22), 0);
        fila.Add(detalle, 1);
        fila.Add(importe, 2);
        return fila;
    }

    /// <summary>Acceso rápido del inicio: ícono, título, detalle y flecha.</summary>
    public static View Accion(string icono, string titulo, string detalle, Action alPulsar)
    {
        var textos = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
        textos.Add(new Label { Text = titulo, FontSize = 16, FontFamily = "OpenSansSemibold", TextColor = Colores.TextoOscuro });
        textos.Add(Secundario(detalle, 13));

        Grid fila = FilaBase(12);
        fila.Add(Circulo(icono, Colores.Primario, Colores.PrimarioSuave, 44, 24), 0);
        fila.Add(textos, 1);
        fila.Add(new Icono { Datos = Iconos.Chevron, Color = Colores.BordeCampo }, 2);
        fila.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(alPulsar) });
        SemanticProperties.SetDescription(fila, $"{titulo}. {detalle}");
        return fila;
    }

    /// <summary>Dato del comprobante: etiqueta a la izquierda y valor a la derecha.</summary>
    public static View Dato(string etiqueta, string valor)
    {
        var fila = new Grid
        {
            Padding = new Thickness(16, 12),
            ColumnSpacing = 12,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) },
        };
        fila.Add(Secundario(etiqueta, 14), 0);
        fila.Add(new Label
        {
            Text = valor,
            FontSize = 14,
            FontFamily = "OpenSansSemibold",
            TextColor = Colores.TextoOscuro,
            HorizontalTextAlignment = TextAlignment.End,
        }, 1);
        return fila;
    }

    /// <summary>Agrega las filas a una lista con un divisor entre cada una.</summary>
    public static void Llenar(Layout lista, IEnumerable<View> filas)
    {
        lista.Children.Clear();
        foreach (View fila in filas)
        {
            if (lista.Children.Count > 0)
                lista.Add(new BoxView { HeightRequest = 1, Color = Colores.BordeTarjeta, Margin = new Thickness(16, 0) });
            lista.Add(fila);
        }
    }

    public static Border Circulo(string icono, Color color, Color fondo, double lado, double tamanoIcono)
        => new()
        {
            WidthRequest = lado,
            HeightRequest = lado,
            StrokeThickness = 0,
            StrokeShape = new Ellipse(),
            BackgroundColor = fondo,
            VerticalOptions = LayoutOptions.Center,
            Content = new Icono { Datos = icono, Color = color, Tamano = tamanoIcono },
        };

    private static Grid FilaBase(double margenDerecho) => new()
    {
        Padding = new Thickness(16, 12, margenDerecho, 12),
        ColumnSpacing = 14,
        MinimumHeightRequest = 72,
        ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
    };

    private static Label Secundario(string texto, double tamano)
        => new() { Text = texto, FontSize = tamano, TextColor = Colores.TextoSecundario };
}
