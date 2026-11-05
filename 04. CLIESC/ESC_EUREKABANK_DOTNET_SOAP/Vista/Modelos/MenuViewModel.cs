using System.Windows.Input;
using ec.edu.monster.modelo;

namespace ec.edu.monster.vista.modelos;

/// <summary>Screens reachable from the menu.</summary>
public enum ModoOperacion
{
    Deposito,
    Retiro,
    Transferencia,
    Movimientos,
}

/// <summary>
/// Home screen: greeting and one card per operation, with the same texts as the other desktop
/// clients. Deposit is employee-only; the history screen is wired in the next work unit.
/// </summary>
public sealed class MenuViewModel
{
    public const string MovimientosTitulo = "Consultar movimientos";
    public const string DepositoTitulo = "Depósito";
    public const string RetiroTitulo = "Retiro";
    public const string TransferenciaTitulo = "Transferencia";

    public const string Proximamente = "Próximamente";

    private readonly Action<ModoOperacion> _abrir;

    public MenuViewModel(Sesion sesion, Action<ModoOperacion> abrir)
    {
        _abrir = abrir;
        Nombre = sesion.NombreVisible;
        EsEmpleado = sesion.EsEmpleado;
        AbrirCommand = new RelayCommand(Abrir);
    }

    public string Nombre { get; }

    public bool EsEmpleado { get; }

    public bool PuedeDepositar => EsEmpleado;

    /// <summary>The history screen is enabled once its work unit lands.</summary>
    public bool PuedeConsultar => false;

    public string MovimientosDetalle
        => PuedeConsultar ? "Consulte el saldo y los movimientos de una cuenta" : Proximamente;

    public string DepositoDetalle
        => EsEmpleado ? "Registre un depósito en la cuenta de un cliente" : Proximamente;

    public string RetiroDetalle
        => EsEmpleado ? "Registre un retiro de la cuenta de un cliente" : "Registrar una salida de su cuenta";

    public string TransferenciaDetalle
        => EsEmpleado ? "Mueva dinero de una cuenta a otra" : "Mover fondos desde su cuenta";

    public ICommand AbrirCommand { get; }

    private void Abrir(object? parametro) => _abrir(Modo(parametro));

    private static ModoOperacion Modo(object? parametro) => (parametro as string) switch
    {
        "deposito" => ModoOperacion.Deposito,
        "retiro" => ModoOperacion.Retiro,
        "transferencia" => ModoOperacion.Transferencia,
        _ => ModoOperacion.Movimientos,
    };
}
