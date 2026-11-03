using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Data;
using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Enums;
using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Interfaces;
using MOV_EUREKABANK_DOTNET_SOAP.Servicios;

namespace MOV_EUREKABANK_DOTNET_SOAP.Controlador;

/// <summary>
/// ECUD3 Registrar Movimiento. El empleado registra depósitos, retiros y transferencias e ingresa la
/// clave de la cuenta; el cliente solo retira o transfiere desde la cuenta de su sesión, sin clave.
/// </summary>
public class OperacionControlador(IMovimientoRepositorio repositorio, SesionServicio sesion) : ControladorConSesion(sesion)
{
    private readonly IMovimientoRepositorio _repositorio = repositorio;
    private readonly MovimientosControlador _consulta = new(repositorio, sesion);

    public IReadOnlyList<TipoOperacion> TiposDisponibles() => EsEmpleado
        ? Enum.GetValues<TipoOperacion>()
        : [TipoOperacion.Retiro, TipoOperacion.Transferencia];

    public bool PideCuenta => EsEmpleado;

    public bool PideDestino(TipoOperacion tipo) => tipo == TipoOperacion.Transferencia;

    public bool PideClave(TipoOperacion tipo) => EsEmpleado && tipo != TipoOperacion.Deposito;

    /// <summary>ECUD3 paso 4 y FA7: valida el formato en el orden de los campos del formulario.</summary>
    public ValidacionResultado Validar(TipoOperacion tipo, string? cuentaIngresada, string? destinoIngresado,
                                      string? importeIngresado, string? claveIngresada)
    {
        if (!TiposDisponibles().Contains(tipo))
            return ValidacionResultado.Error(Campo.Ninguno, "Operación no permitida para su perfil");

        string cuenta = CuentaOrigen(cuentaIngresada);
        if (PideCuenta)
        {
            string? error = ValidadorDatos.ErrorCuenta(cuenta);
            if (error is not null)
                return ValidacionResultado.Error(Campo.Cuenta, error);
        }

        if (PideDestino(tipo))
        {
            string destino = ValidadorDatos.Limpiar(destinoIngresado);
            string? error = ValidadorDatos.ErrorCuenta(destino);
            if (error is not null)
                return ValidacionResultado.Error(Campo.Destino, error);
            if (destino == cuenta)
                return ValidacionResultado.Error(Campo.Destino, ValidadorDatos.CuentasIguales);
        }

        string? errorImporte = ValidadorDatos.ErrorImporte(ValidadorDatos.Limpiar(importeIngresado));
        if (errorImporte is not null)
            return ValidacionResultado.Error(Campo.Importe, errorImporte);

        if (PideClave(tipo))
        {
            string? error = ValidadorDatos.ErrorClaveCuenta(ValidadorDatos.Limpiar(claveIngresada));
            if (error is not null)
                return ValidacionResultado.Error(Campo.Clave, error);
        }
        return ValidacionResultado.Valida;
    }

    /// <summary>Construye la solicitud a partir de datos que ya pasaron por <see cref="Validar"/>.</summary>
    public SolicitudOperacion Preparar(TipoOperacion tipo, string? cuentaIngresada, string? destinoIngresado,
                                       string? importeIngresado, string? claveIngresada)
    {
        ValidacionResultado validacion = Validar(tipo, cuentaIngresada, destinoIngresado, importeIngresado, claveIngresada);
        if (!validacion.Valido)
            throw new ArgumentException(validacion.Mensaje);

        return new SolicitudOperacion(tipo,
            CuentaOrigen(cuentaIngresada),
            PideDestino(tipo) ? ValidadorDatos.Limpiar(destinoIngresado) : null,
            ValidadorDatos.ConvertirImporte(importeIngresado)!.Value,
            PideClave(tipo) ? ValidadorDatos.Limpiar(claveIngresada) : "");
    }

    public async Task<Resultado<Comprobante>> RegistrarAsync(SolicitudOperacion solicitud)
    {
        if (!HaySesion)
            return Resultado<Comprobante>.Expirada(MensajeSesionExpirada);

        Resultado<Comprobante> resultado = RevisarSesion(await _repositorio.RegistrarAsync(Token!, solicitud));
        if (resultado.Exito && !EsEmpleado)
            _sesion.ActualizarSaldoCliente(resultado.Datos!.NuevoSaldo);
        return resultado;
    }

    /// <summary>Cuenta del cliente para mostrar el número y el saldo junto al formulario.</summary>
    public Cuenta? CuentaClienteGuardada => _sesion.CuentaCliente;

    public async Task<Resultado<Cuenta>> CargarCuentaClienteAsync()
    {
        Resultado<ConsultaMovimientos> resultado = await _consulta.ConsultarAsync(null);
        if (resultado.Exito)
            return Resultado<Cuenta>.Ok(_sesion.CuentaCliente!, resultado.Mensaje);
        return resultado.SesionExpirada
            ? Resultado<Cuenta>.Expirada(resultado.Mensaje)
            : Resultado<Cuenta>.Error(resultado.Mensaje);
    }

    private string CuentaOrigen(string? cuentaIngresada)
        => PideCuenta ? ValidadorDatos.Limpiar(cuentaIngresada) : CuentaSesion ?? "";
}
