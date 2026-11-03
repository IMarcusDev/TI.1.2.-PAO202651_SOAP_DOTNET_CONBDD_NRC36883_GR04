using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Data;
using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Enums;
using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Interfaces;
using MOV_EUREKABANK_DOTNET_SOAP.Servicios;

namespace MOV_EUREKABANK_DOTNET_SOAP.Controlador;

public class LoginControlador(ILoginRepositorio repositorio, SesionServicio sesion) : ControladorConSesion(sesion)
{
    private readonly ILoginRepositorio _repositorio = repositorio;

    /// <summary>ECUD1 paso 4 y FA3: campos vacíos y, para el cliente, cuenta de 8 dígitos y clave de 6.</summary>
    public ValidacionResultado Validar(Perfil perfil, string? credencialIngresada, string? claveIngresada)
    {
        string credencial = ValidadorDatos.Limpiar(credencialIngresada);
        string clave = ValidadorDatos.Limpiar(claveIngresada);

        if (perfil == Perfil.Cliente)
        {
            string? errorCuenta = ValidadorDatos.ErrorCuenta(credencial);
            if (errorCuenta is not null)
                return ValidacionResultado.Error(Campo.Credencial, errorCuenta);

            string? errorClave = ValidadorDatos.ErrorClaveCuenta(clave);
            return errorClave is null ? ValidacionResultado.Valida : ValidacionResultado.Error(Campo.Clave, errorClave);
        }

        if (credencial.Length == 0)
            return ValidacionResultado.Error(Campo.Credencial, ValidadorDatos.IngreseUsuario);
        if (clave.Length == 0)
            return ValidacionResultado.Error(Campo.Clave, ValidadorDatos.IngreseClave);
        return ValidacionResultado.Valida;
    }

    public async Task<Resultado<Sesion>> IniciarSesionAsync(Perfil perfil, string? credencialIngresada, string? claveIngresada)
    {
        ValidacionResultado validacion = Validar(perfil, credencialIngresada, claveIngresada);
        if (!validacion.Valido)
            return Resultado<Sesion>.Error(validacion.Mensaje!);

        string credencial = ValidadorDatos.Limpiar(credencialIngresada);
        string clave = ValidadorDatos.Limpiar(claveIngresada);
        Resultado<Sesion> resultado = perfil == Perfil.Cliente
            ? await _repositorio.LoginClienteAsync(credencial, clave)
            : await _repositorio.LoginEmpleadoAsync(credencial, clave);

        if (resultado.Exito)
            _sesion.Iniciar(resultado.Datos!);
        return resultado;
    }

    /// <summary>
    /// ECUD1 FA5: borra la sesión local de inmediato, para que el usuario vuelva al inicio de sesión
    /// aunque el servidor no responda.
    /// </summary>
    /// <returns>El token, para avisar al servidor con <see cref="NotificarCierreAsync"/>.</returns>
    public string? CerrarSesion()
    {
        string? token = Token;
        _sesion.Limpiar();
        return token;
    }

    /// <summary>Envía logout(token) al servidor. Se ejecuta en segundo plano; su resultado no cambia el flujo.</summary>
    public Task<Resultado<bool>> NotificarCierreAsync(string? token)
    {
        if (string.IsNullOrEmpty(token))
            return Task.FromResult(Resultado<bool>.Ok(true, ""));
        return _repositorio.LogoutAsync(token);
    }
}
