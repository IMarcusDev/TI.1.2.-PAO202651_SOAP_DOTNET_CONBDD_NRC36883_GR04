namespace MOV_EUREKABANK_DOTNET_SOAP.Vista.Componentes;

/// <summary>
/// Campo con contorno, ícono, título, texto de ayuda y mensaje de error, como el TextInputLayout del cliente
/// Android. El error se borra en cuanto el usuario vuelve a escribir.
/// </summary>
public class CampoTexto : ContentView
{
    public static readonly BindableProperty TituloProperty =
        BindableProperty.Create(nameof(Titulo), typeof(string), typeof(CampoTexto), "", propertyChanged: (b, _, n) => ((CampoTexto)b)._titulo.Text = (string)n);

    public static readonly BindableProperty IconoDatosProperty =
        BindableProperty.Create(nameof(IconoDatos), typeof(string), typeof(CampoTexto), null, propertyChanged: (b, _, n) => ((CampoTexto)b).AplicarIcono((string?)n));

    public static readonly BindableProperty AyudaProperty =
        BindableProperty.Create(nameof(Ayuda), typeof(string), typeof(CampoTexto), null, propertyChanged: (b, _, _) => ((CampoTexto)b).ActualizarPie());

    public static readonly BindableProperty ErrorProperty =
        BindableProperty.Create(nameof(Error), typeof(string), typeof(CampoTexto), null, propertyChanged: (b, _, _) => ((CampoTexto)b).ActualizarPie());

    public static readonly BindableProperty EsClaveProperty =
        BindableProperty.Create(nameof(EsClave), typeof(bool), typeof(CampoTexto), false, propertyChanged: (b, _, n) => ((CampoTexto)b).AplicarClave((bool)n));

    public static readonly BindableProperty TecladoProperty =
        BindableProperty.Create(nameof(Teclado), typeof(Keyboard), typeof(CampoTexto), Keyboard.Default, propertyChanged: (b, _, n) => ((CampoTexto)b)._entrada.Keyboard = (Keyboard)n);

    public static readonly BindableProperty LargoMaximoProperty =
        BindableProperty.Create(nameof(LargoMaximo), typeof(int), typeof(CampoTexto), int.MaxValue, propertyChanged: (b, _, n) => ((CampoTexto)b)._entrada.MaxLength = (int)n);

    public static readonly BindableProperty TipoRetornoProperty =
        BindableProperty.Create(nameof(TipoRetorno), typeof(ReturnType), typeof(CampoTexto), ReturnType.Default, propertyChanged: (b, _, n) => ((CampoTexto)b)._entrada.ReturnType = (ReturnType)n);

    private readonly Border _caja;
    private readonly Icono _icono = new() { Tamano = 22, Margin = new Thickness(0, 0, 12, 0) };
    private readonly Label _titulo = new() { FontSize = 12 };
    private readonly Entry _entrada = new() { FontSize = 16, Margin = new Thickness(-4, -6, 0, -4) };
    private readonly Icono _ojo = new() { Tamano = 22, Datos = Iconos.Ver, IsVisible = false };
    private readonly Grid _botonOjo;
    private readonly Label _pie = new() { FontSize = 12, Margin = new Thickness(14, 4, 14, 0), IsVisible = false };

    public CampoTexto()
    {
        _icono.Color = Colores.TextoSecundario;
        _icono.IsVisible = false;
        _ojo.Color = Colores.TextoSecundario;
        _titulo.TextColor = Colores.TextoSecundario;

        _botonOjo = new Grid { WidthRequest = 40, HeightRequest = 40, IsVisible = false, Children = { _ojo } };
        _botonOjo.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(AlternarClave) });
        SemanticProperties.SetDescription(_botonOjo, "Mostrar u ocultar la clave");

        var textos = new VerticalStackLayout { Spacing = 0, VerticalOptions = LayoutOptions.Center, Children = { _titulo, _entrada } };
        var fila = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
        };
        fila.Add(_icono, 0);
        fila.Add(textos, 1);
        fila.Add(_botonOjo, 2);

        _caja = new Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 },
            StrokeThickness = 1,
            Stroke = Colores.BordeCampo,
            BackgroundColor = Colors.White,
            Padding = new Thickness(14, 6, 6, 6),
            MinimumHeightRequest = 60,
            Content = fila,
        };
        _caja.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => _entrada.Focus()) });

        _entrada.TextChanged += (_, e) =>
        {
            if (Error is not null)
                Error = null;
            TextoCambiado?.Invoke(this, e);
        };
        _entrada.Completed += (_, _) => Completado?.Invoke(this, EventArgs.Empty);
        _entrada.Focused += (_, _) => ActualizarBorde();
        _entrada.Unfocused += (_, _) => ActualizarBorde();

        Content = new VerticalStackLayout { Spacing = 0, Children = { _caja, _pie } };
    }

    public event EventHandler? Completado;

    public event EventHandler<TextChangedEventArgs>? TextoCambiado;

    public string Titulo { get => (string)GetValue(TituloProperty); set => SetValue(TituloProperty, value); }

    public string? IconoDatos { get => (string?)GetValue(IconoDatosProperty); set => SetValue(IconoDatosProperty, value); }

    public string? Ayuda { get => (string?)GetValue(AyudaProperty); set => SetValue(AyudaProperty, value); }

    public string? Error { get => (string?)GetValue(ErrorProperty); set => SetValue(ErrorProperty, value); }

    public bool EsClave { get => (bool)GetValue(EsClaveProperty); set => SetValue(EsClaveProperty, value); }

    public Keyboard Teclado { get => (Keyboard)GetValue(TecladoProperty); set => SetValue(TecladoProperty, value); }

    public int LargoMaximo { get => (int)GetValue(LargoMaximoProperty); set => SetValue(LargoMaximoProperty, value); }

    public ReturnType TipoRetorno { get => (ReturnType)GetValue(TipoRetornoProperty); set => SetValue(TipoRetornoProperty, value); }

    public string Texto
    {
        get => _entrada.Text ?? "";
        set => _entrada.Text = value;
    }

    public void Enfocar() => _entrada.Focus();

    public void QuitarFoco() => _entrada.Unfocus();

    /// <summary>Muestra el error bajo el campo y lleva el foco a él.</summary>
    public void MostrarError(string? mensaje)
    {
        Error = mensaje;
        Enfocar();
    }

    private void AplicarIcono(string? datos)
    {
        _icono.Datos = datos;
        _icono.IsVisible = !string.IsNullOrEmpty(datos);
    }

    /// <summary>Al cambiar entre texto y clave se vuelve a ocultar el contenido, como el password_toggle de Android.</summary>
    private void AplicarClave(bool esClave)
    {
        _entrada.IsPassword = esClave;
        _ojo.Datos = Iconos.Ver;
        _ojo.IsVisible = esClave;
        _botonOjo.IsVisible = esClave;
    }

    private void AlternarClave()
    {
        _entrada.IsPassword = !_entrada.IsPassword;
        _ojo.Datos = _entrada.IsPassword ? Iconos.Ver : Iconos.Ocultar;
    }

    private void ActualizarPie()
    {
        bool hayError = !string.IsNullOrEmpty(Error);
        _pie.Text = hayError ? Error : Ayuda;
        _pie.TextColor = hayError ? Colores.TextoError : Colores.TextoSecundario;
        _pie.IsVisible = !string.IsNullOrEmpty(_pie.Text);
        ActualizarBorde();
    }

    private void ActualizarBorde()
    {
        bool hayError = !string.IsNullOrEmpty(Error);
        _caja.Stroke = hayError ? Colores.TextoError : _entrada.IsFocused ? Colores.Primario : Colores.BordeCampo;
        _caja.StrokeThickness = hayError || _entrada.IsFocused ? 2 : 1;
        _titulo.TextColor = hayError ? Colores.TextoError : _entrada.IsFocused ? Colores.Primario : Colores.TextoSecundario;
        _icono.Color = hayError ? Colores.TextoError : Colores.TextoSecundario;
    }
}
