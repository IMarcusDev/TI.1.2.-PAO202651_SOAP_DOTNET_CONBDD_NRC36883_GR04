using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Enums;
using MOV_EUREKABANK_DOTNET_SOAP.Vista.Componentes;

namespace MOV_EUREKABANK_DOTNET_SOAP.Vista;

/// <summary>Textos de la interfaz (strings.xml del cliente Android).</summary>
public static class Textos
{
    public const string AppNombre = "EurekaBank";

    public const string CerrarSesion = "Cerrar sesión";
    public const string CerrarSesionPregunta = "¿Desea cerrar la sesión?";
    public const string Si = "Sí";
    public const string No = "No";
    public const string Sesion = "Sesión";
    public const string Aceptar = "Aceptar";
    public const string Cancelar = "Cancelar";
    public const string ErrorInesperado = "Ocurrió un error inesperado. Intente de nuevo.";

    public const string LoginUsuario = "Usuario";
    public const string LoginCuenta = "Número de cuenta";
    public const string LoginClave = "Clave";
    public const string LoginClaveCuenta = "Clave de la cuenta";
    public const string LoginAyudaCuenta = "8 dígitos";
    public const string LoginAyudaClaveCuenta = "6 dígitos";
    public const string LoginServidor = "Servidor: {0}";

    public const string PestanaInicio = "Inicio";
    public const string PestanaMovimientos = "Movimientos";
    public const string PestanaOperaciones = "Operaciones";

    public const string InicioPerfil = "Perfil {0}";
    public const string InicioNumeroCuenta = "N.º {0}";
    public const string InicioCargandoCuenta = "Cargando su cuenta…";

    public const string AccionConsultar = "Consultar movimientos";
    public const string AccionConsultarDetalleEmpleado = "Datos y movimientos de cualquier cuenta";
    public const string AccionConsultarDetalleCliente = "Historial de su cuenta";

    public const string MovNumeroFecha = "N.º {0} · {1}";
    public const string MovReferencia = "Cuenta ref. {0}";
    public const string MovIngreso = "Ingreso";
    public const string MovSalida = "Salida";

    public const string OpCuenta = "Número de cuenta";
    public const string OpCuentaOrigen = "Cuenta de origen";
    public const string OpClaveCuenta = "Clave de la cuenta";
    public const string OpClaveCuentaOrigen = "Clave de la cuenta de origen";
    public const string OpSaldoDisponible = "Saldo disponible: {0}";
    public const string OpConfirmarTitulo = "Confirmar operación";
    public const string OpConfirmarDeposito = "¿Depositar {0} en la cuenta {1}?";
    public const string OpConfirmarRetiro = "¿Retirar {0} de la cuenta {1}?";
    public const string OpConfirmarTransferencia = "¿Transferir {0} de la cuenta {1} a la cuenta {2}?";
    public const string OpConfirmar = "Confirmar";

    public const string CompOperacion = "Operación";
    public const string CompCuenta = "Cuenta";
    public const string CompCuentaOrigen = "Cuenta de origen";
    public const string CompCuentaDestino = "Cuenta de destino";
    public const string CompNumero = "N.º de movimiento";
    public const string CompNuevoSaldo = "Nuevo saldo";
    public const string CompFecha = "Fecha";

    public static string MovCantidad(int cantidad) => cantidad == 1 ? "1 movimiento" : $"{cantidad} movimientos";

    public static string NombreOperacion(TipoOperacion tipo) => tipo switch
    {
        TipoOperacion.Deposito => "Depósito",
        TipoOperacion.Retiro => "Retiro",
        _ => "Transferencia",
    };

    public static string VerboOperacion(TipoOperacion tipo) => tipo switch
    {
        TipoOperacion.Deposito => "Depositar",
        TipoOperacion.Retiro => "Retirar",
        _ => "Transferir",
    };

    public static string DescripcionOperacion(TipoOperacion tipo) => tipo switch
    {
        TipoOperacion.Deposito => "Acreditar dinero entregado en ventanilla",
        TipoOperacion.Retiro => "Debitar dinero de una cuenta",
        _ => "Enviar dinero a otra cuenta del banco",
    };

    public static string IconoOperacion(TipoOperacion tipo) => tipo switch
    {
        TipoOperacion.Deposito => Iconos.Deposito,
        TipoOperacion.Retiro => Iconos.Retiro,
        _ => Iconos.Transferir,
    };

    /// <summary>Ícono blanco del botón de registrar (Resources/Images).</summary>
    public static string ImagenOperacion(TipoOperacion tipo) => tipo switch
    {
        TipoOperacion.Deposito => "btn_deposito.png",
        TipoOperacion.Retiro => "btn_retiro.png",
        _ => "btn_transferir.png",
    };
}
