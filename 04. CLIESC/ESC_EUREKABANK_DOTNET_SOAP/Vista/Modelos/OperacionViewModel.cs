using System.ComponentModel;
using System.Windows.Input;
using ec.edu.monster.controlador;
using ec.edu.monster.modelo;

namespace ec.edu.monster.vista.modelos;

/// <summary>
/// One movement screen (deposit, withdrawal or transfer). The same view model serves the three
/// operations: the mode and the role decide which fields are shown and how the form is built.
/// </summary>
public sealed class OperacionViewModel : INotifyPropertyChanged
{
    private static readonly string[] Campos = ["cuenta", "origen", "destino", "clave", "importe"];

    private readonly MovimientoControlador _controlador;
    private readonly Action<Comprobante> _alComprobante;
    private readonly Func<string, bool> _confirmar;
    private readonly Action _alVolver;
    private readonly AsyncRelayCommand _revisar;

    private string _cuenta = "";
    private string _origen = "";
    private string _destino = "";
    private string _clave = "";
    private string _importe = "";
    private string _errorGeneral = "";
    private bool _ocupado;
    private IReadOnlyDictionary<string, string> _errores = SinErrores();

    public OperacionViewModel(MovimientoControlador controlador, ModoOperacion modo, bool esCliente,
        string cuentaCliente, Action<Comprobante> alComprobante, Func<string, bool> confirmar, Action alVolver)
    {
        _controlador = controlador;
        _alComprobante = alComprobante;
        _confirmar = confirmar;
        _alVolver = alVolver;
        _revisar = new AsyncRelayCommand(RevisarAsync, () => !Ocupado);

        Modo = modo;
        EsCliente = esCliente;
        CuentaFija = esCliente ? cuentaCliente : "";

        RevisarCommand = _revisar;
        LimpiarCommand = new RelayCommand(_ => Limpiar());
        VolverCommand = new RelayCommand(_ => _alVolver());
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ModoOperacion Modo { get; }

    public bool EsCliente { get; }

    /// <summary>Own account of a Cliente: shown read-only, never edited.</summary>
    public string CuentaFija { get; }

    public string Titulo => Modo switch
    {
        ModoOperacion.Deposito => "Depósito",
        ModoOperacion.Retiro => "Retiro",
        _ => "Transferencia",
    };

    public string Descripcion => Modo switch
    {
        ModoOperacion.Deposito => "Registre un depósito en la cuenta de un cliente.",
        ModoOperacion.Retiro => EsCliente
            ? "Registrar una salida de su cuenta."
            : "Registre un retiro de la cuenta de un cliente.",
        _ => EsCliente
            ? "Mover fondos desde su cuenta. La cuenta de destino debe ser diferente."
            : "Mueva dinero de una cuenta a otra. Las cuentas de origen y destino deben ser diferentes.",
    };

    public string EtiquetaCuenta => Modo == ModoOperacion.Deposito ? "Cuenta de destino" : "Cuenta de origen";

    public string EtiquetaClave => Modo == ModoOperacion.Transferencia
        ? "Clave de la cuenta de origen"
        : "Clave de la cuenta";

    public bool MuestraCuenta => Modo == ModoOperacion.Deposito
                                 || (Modo == ModoOperacion.Retiro && !EsCliente);

    public bool MuestraCuentaFija => Modo == ModoOperacion.Retiro && EsCliente;

    public bool MuestraOrigen => Modo == ModoOperacion.Transferencia && !EsCliente;

    public bool MuestraOrigenFijo => Modo == ModoOperacion.Transferencia && EsCliente;

    public bool MuestraDestino => Modo == ModoOperacion.Transferencia;

    public bool MuestraClave => !EsCliente && Modo != ModoOperacion.Deposito;

    public string Cuenta
    {
        get => _cuenta;
        set => Cambiar(ref _cuenta, value);
    }

    public string Origen
    {
        get => _origen;
        set => Cambiar(ref _origen, value);
    }

    public string Destino
    {
        get => _destino;
        set => Cambiar(ref _destino, value);
    }

    public string Clave
    {
        get => _clave;
        set => Cambiar(ref _clave, value);
    }

    public string Importe
    {
        get => _importe;
        set => Cambiar(ref _importe, value);
    }

    /// <summary>Per-field messages, keyed by form field name (cuenta, origen, destino, clave, importe).</summary>
    public IReadOnlyDictionary<string, string> Errores
    {
        get => _errores;
        private set
        {
            _errores = value;
            OnPropertyChanged(nameof(Errores));
        }
    }

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
            OnPropertyChanged(nameof(PuedeRevisar));
            _revisar.RaiseCanExecuteChanged();
        }
    }

    public bool PuedeRevisar => !_ocupado;

    public ICommand RevisarCommand { get; }

    public ICommand LimpiarCommand { get; }

    public ICommand VolverCommand { get; }

    public async Task RevisarAsync()
    {
        if (Ocupado)
            return;

        Ocupado = true;
        ErrorGeneral = "";
        Errores = SinErrores();
        try
        {
            MovimientoForm form = Leer();
            ResultadoRevision resultado = await _controlador.RevisarAsync(form, _confirmar);
            switch (resultado.Estado)
            {
                case EstadoRevision.Invalido:
                    Errores = ConErrores(form.Errores);
                    break;
                case EstadoRevision.Error:
                    ErrorGeneral = resultado.Mensaje ?? "";
                    break;
                case EstadoRevision.Exito:
                    _alComprobante(resultado.Comprobante!);
                    Limpiar();
                    break;
            }
        }
        finally
        {
            Ocupado = false;
        }
    }

    public void Limpiar()
    {
        Cuenta = "";
        Origen = "";
        Destino = "";
        Clave = "";
        Importe = "";
        Errores = SinErrores();
        ErrorGeneral = "";
    }

    /// <summary>Every field gets a key, so the view never binds a missing one.</summary>
    private static IReadOnlyDictionary<string, string> SinErrores()
        => Campos.ToDictionary(campo => campo, _ => "");

    private static IReadOnlyDictionary<string, string> ConErrores(IReadOnlyDictionary<string, string> errores)
    {
        Dictionary<string, string> mapa = Campos.ToDictionary(campo => campo, _ => "");
        foreach (KeyValuePair<string, string> par in errores)
            mapa[par.Key] = par.Value;
        return mapa;
    }

    private MovimientoForm Leer() => Modo switch
    {
        ModoOperacion.Deposito => MovimientoForm.Deposito(Cuenta, Importe),
        ModoOperacion.Retiro => EsCliente
            ? MovimientoForm.Retiro(CuentaFija, Importe)
            : MovimientoForm.RetiroEmpleado(Cuenta, Clave, Importe),
        _ => EsCliente
            ? MovimientoForm.Transferencia(CuentaFija, Destino, Importe)
            : MovimientoForm.TransferenciaEmpleado(Origen, Destino, Clave, Importe),
    };

    private void Cambiar(ref string campo, string valor, [System.Runtime.CompilerServices.CallerMemberName] string nombre = "")
    {
        if (campo == valor)
            return;
        campo = valor;
        OnPropertyChanged(nombre);
    }

    private void OnPropertyChanged(string nombre)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombre));
}
