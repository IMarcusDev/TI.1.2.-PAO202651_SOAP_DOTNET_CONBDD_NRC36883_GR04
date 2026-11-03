using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Data;
using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Enums;
using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Interfaces;
using MOV_EUREKABANK_DOTNET_SOAP.Servicios;

namespace MOV_EUREKABANK_DOTNET_SOAP.Controlador;

/// <summary>ECUD2 Consultar Movimientos: el empleado consulta cualquier cuenta; el cliente, solo la suya.</summary>
public class MovimientosControlador(IMovimientoRepositorio repositorio, SesionServicio sesion) : ControladorConSesion(sesion)
{
    private readonly IMovimientoRepositorio _repositorio = repositorio;

    public ValidacionResultado Validar(string? cuentaIngresada)
    {
        if (!EsEmpleado)
            return ValidacionResultado.Valida;

        string? error = ValidadorDatos.ErrorCuenta(ValidadorDatos.Limpiar(cuentaIngresada));
        return error is null ? ValidacionResultado.Valida : ValidacionResultado.Error(Campo.Cuenta, error);
    }

    /// <param name="cuentaIngresada">Se ignora para el cliente, que siempre consulta la cuenta de su sesión.</param>
    public async Task<Resultado<ConsultaMovimientos>> ConsultarAsync(string? cuentaIngresada)
    {
        if (!HaySesion)
            return Resultado<ConsultaMovimientos>.Expirada(MensajeSesionExpirada);

        ValidacionResultado validacion = Validar(cuentaIngresada);
        if (!validacion.Valido)
            return Resultado<ConsultaMovimientos>.Error(validacion.Mensaje!);

        string cuenta = EsEmpleado ? ValidadorDatos.Limpiar(cuentaIngresada) : CuentaSesion ?? "";
        Resultado<ConsultaMovimientos> resultado = RevisarSesion(await _repositorio.ConsultarMovimientosAsync(Token!, cuenta));

        if (resultado.Exito && !EsEmpleado)
            _sesion.ActualizarCuentaCliente(resultado.Datos!.Cuenta);
        return resultado;
    }
}
