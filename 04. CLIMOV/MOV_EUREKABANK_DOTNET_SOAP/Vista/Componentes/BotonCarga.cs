namespace MOV_EUREKABANK_DOTNET_SOAP.Vista.Componentes;

/// <summary>Botón principal que, mientras se procesa la solicitud, se bloquea y muestra un indicador (RNF-013).</summary>
public class BotonCarga : ContentView
{
    private readonly Button _boton = new();
    private readonly ActivityIndicator _indicador = new()
    {
        Color = Colors.White,
        IsRunning = false,
        IsVisible = false,
        WidthRequest = 24,
        HeightRequest = 24,
        HorizontalOptions = LayoutOptions.Center,
        VerticalOptions = LayoutOptions.Center,
        InputTransparent = true,
    };
    private string _texto = "";
    private ImageSource? _imagen;

    public BotonCarga()
    {
        if (Application.Current?.Resources.TryGetValue("BotonPrimario", out object? estilo) == true)
            _boton.Style = (Style)estilo;
        _boton.Clicked += (_, _) => Pulsado?.Invoke(this, EventArgs.Empty);
        Content = new Grid { Children = { _boton, _indicador } };
    }

    public event EventHandler? Pulsado;

    public string Texto
    {
        get => _texto;
        set
        {
            _texto = value;
            if (!Cargando)
                _boton.Text = value;
            SemanticProperties.SetDescription(_boton, value);
        }
    }

    public ImageSource? Imagen
    {
        get => _imagen;
        set
        {
            _imagen = value;
            if (!Cargando)
                _boton.ImageSource = value;
        }
    }

    public bool Cargando
    {
        get => _indicador.IsRunning;
        set
        {
            _indicador.IsRunning = value;
            _indicador.IsVisible = value;
            _boton.IsEnabled = !value;
            _boton.Text = value ? "" : _texto;
            _boton.ImageSource = value ? null : _imagen;
        }
    }
}
