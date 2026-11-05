using System.ComponentModel;
using System.Windows.Input;
using ec.edu.monster.controlador;
using ec.edu.monster.modelo;
using ec.edu.monster.modelo.dto;
using ec.edu.monster.modelo.servicios;

namespace ec.edu.monster.vista.modelos;

/// <summary>
/// State of the main window: the logged-in session, the home menu, the current screen and the
/// server status. The window only renders whatever Contenido holds.
/// </summary>
public sealed class PrincipalViewModel : INotifyPropertyChanged
{
    private readonly Sesion _sesion;
    private readonly MovimientoServicio _servicio;
    private readonly Func<string, bool> _confirmar;
    private readonly Action<Comprobante> _alComprobante;
    private object _contenido;

    public PrincipalViewModel(Sesion sesion, MovimientoServicio servicio, EstadoServidorViewModel estado,
        Func<string, bool> confirmar, Action<Comprobante> alComprobante)
    {
        _sesion = sesion;
        _servicio = servicio;
        _confirmar = confirmar;
        _alComprobante = alComprobante;

        Estado = estado;
        Menu = new MenuViewModel(sesion, MostrarOperacion);
        _contenido = Menu;
        MostrarMenuCommand = new RelayCommand(_ => MostrarMenu());
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public Sesion Sesion => _sesion;

    public EstadoServidorViewModel Estado { get; }

    public MenuViewModel Menu { get; }

    public object Contenido
    {
        get => _contenido;
        private set
        {
            _contenido = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Contenido)));
        }
    }

    public ICommand MostrarMenuCommand { get; }

    public void MostrarMenu() => Contenido = Menu;

    private void MostrarOperacion(ModoOperacion modo)
    {
        if (modo == ModoOperacion.Movimientos)
        {
            Contenido = new MovimientosViewModel(new MovimientosControlador(_sesion, _servicio),
                _sesion.EsCliente, _sesion.Cuenta, MostrarMenu);
            return;
        }

        Contenido = new OperacionViewModel(CrearControlador(modo), modo, _sesion.EsCliente, _sesion.Cuenta,
            _alComprobante, _confirmar, MostrarMenu);
    }

    private MovimientoControlador CrearControlador(ModoOperacion modo) => modo switch
    {
        ModoOperacion.Deposito => new MovimientoControlador(_sesion,
            form => $"¿Depositar {form.ImporteFormateado} en la cuenta {form.Cuenta}?",
            (token, form) => _servicio.RegistrarDepositoAsync(token, form.Cuenta, form.Importe ?? 0m),
            Comprobante.Deposito),

        ModoOperacion.Retiro => new MovimientoControlador(_sesion,
            form => $"¿Retirar {form.ImporteFormateado} de la cuenta {form.Cuenta}?",
            (token, form) => _servicio.RegistrarRetiroAsync(token, form.Cuenta, form.Importe ?? 0m, form.Clave),
            Comprobante.Retiro),

        _ => new MovimientoControlador(_sesion,
            form => $"¿Transferir {form.ImporteFormateado} de la cuenta {form.Origen} a la cuenta {form.Destino}?",
            (token, form) => _servicio.RegistrarTransferenciaAsync(token, form.Origen, form.Destino,
                form.Importe ?? 0m, form.Clave),
            Comprobante.Transferencia),
    };
}
