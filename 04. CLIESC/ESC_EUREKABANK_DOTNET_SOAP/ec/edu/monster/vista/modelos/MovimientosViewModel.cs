using System.ComponentModel;
using System.Windows.Input;
using ec.edu.monster.controlador;
using ec.edu.monster.modelo.dto;

namespace ec.edu.monster.vista.modelos;

/// <summary>
/// "Consultar movimientos" screen. An Empleado types any account; a Cliente only sees their own,
/// loaded as soon as the screen opens. The answer shows the account data above a read-only table,
/// or the message that the account has no movements.
/// </summary>
public sealed class MovimientosViewModel : INotifyPropertyChanged
{
    private readonly MovimientosControlador _controlador;
    private readonly AsyncRelayCommand _consultar;

    private string _cuenta;
    private string _errorGeneral = "";
    private bool _ocupado;
    private HistorialCuenta? _historial;

    public MovimientosViewModel(MovimientosControlador controlador, bool esCliente, string cuentaCliente,
        Action alVolver)
    {
        _controlador = controlador;
        _consultar = new AsyncRelayCommand(ConsultarAsync, () => !Ocupado);
        EsCliente = esCliente;
        _cuenta = esCliente ? cuentaCliente : "";

        ConsultarCommand = _consultar;
        VolverCommand = new RelayCommand(_ => alVolver());
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool EsCliente { get; }

    /// <summary>A Cliente has no search form: the account comes from the session.</summary>
    public bool MuestraBusqueda => !EsCliente;

    public string Cuenta
    {
        get => _cuenta;
        set
        {
            if (_cuenta == value)
                return;
            _cuenta = value;
            OnPropertyChanged(nameof(Cuenta));
        }
    }

    /// <summary>Answer of the last query; null until one succeeds.</summary>
    public HistorialCuenta? Historial => _historial;

    public bool HayHistorial => _historial is not null;

    public bool SinMovimientos => _historial is { SinMovimientos: true };

    public bool HayMovimientos => _historial is { SinMovimientos: false };

    public string Numero => _historial?.Cuenta?.Numero ?? "";

    public string Titular => _historial?.Cuenta?.Titular ?? "";

    public string Moneda => _historial?.Cuenta?.Moneda ?? "";

    public string Saldo => _historial?.Cuenta?.SaldoFormateado ?? "";

    public string EstadoCuenta => _historial?.Cuenta?.Estado ?? "";

    public IReadOnlyList<MovimientoConsultado> Movimientos => _historial?.Movimientos ?? [];

    public string ErrorGeneral
    {
        get => _errorGeneral;
        private set
        {
            if (_errorGeneral == value)
                return;
            _errorGeneral = value;
            OnPropertyChanged(nameof(ErrorGeneral));
            OnPropertyChanged(nameof(HayErrorGeneral));
        }
    }

    public bool HayErrorGeneral => !string.IsNullOrEmpty(_errorGeneral);

    public bool Ocupado
    {
        get => _ocupado;
        private set
        {
            if (_ocupado == value)
                return;
            _ocupado = value;
            OnPropertyChanged(nameof(Ocupado));
            OnPropertyChanged(nameof(PuedeConsultar));
            _consultar.RaiseCanExecuteChanged();
        }
    }

    public bool PuedeConsultar => !_ocupado;

    public ICommand ConsultarCommand { get; }

    public ICommand VolverCommand { get; }

    public async Task ConsultarAsync()
    {
        if (Ocupado)
            return;

        Ocupado = true;
        ErrorGeneral = "";
        try
        {
            HistorialCuenta historial = await _controlador.ConsultarAsync(Cuenta);
            if (!historial.Ok)
            {
                Aplicar(null);
                ErrorGeneral = historial.Mensaje;
                return;
            }

            Aplicar(historial);
        }
        finally
        {
            Ocupado = false;
        }
    }

    private void Aplicar(HistorialCuenta? historial)
    {
        _historial = historial;
        OnPropertyChanged(nameof(Historial));
        OnPropertyChanged(nameof(HayHistorial));
        OnPropertyChanged(nameof(SinMovimientos));
        OnPropertyChanged(nameof(HayMovimientos));
        OnPropertyChanged(nameof(Numero));
        OnPropertyChanged(nameof(Titular));
        OnPropertyChanged(nameof(Moneda));
        OnPropertyChanged(nameof(Saldo));
        OnPropertyChanged(nameof(EstadoCuenta));
        OnPropertyChanged(nameof(Movimientos));
    }

    private void OnPropertyChanged(string nombre)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombre));
}
