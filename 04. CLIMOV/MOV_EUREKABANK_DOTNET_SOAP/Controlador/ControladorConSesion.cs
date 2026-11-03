using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Data;
using MOV_EUREKABANK_DOTNET_SOAP.Servicios;

namespace MOV_EUREKABANK_DOTNET_SOAP.Controlador;

public abstract class ControladorConSesion(SesionServicio sesion)
{
    public const string MensajeSesionExpirada = "Su sesión expiró o no es válida. Inicie sesión nuevamente.";

    protected readonly SesionServicio _sesion = sesion;

    protected string? Token => _sesion.Token;

    public bool HaySesion => _sesion.EstaAutenticado;

    public Sesion? Sesion => _sesion.Sesion;

    public bool EsEmpleado => _sesion.Sesion?.EsEmpleado == true;

    /// <summary>Cuenta con la que inició sesión el cliente; null para el empleado.</summary>
    public string? CuentaSesion => _sesion.Sesion?.Cuenta;

    /// <summary>ECUD4 FA1: si el servidor rechazó el token, se borra la sesión local.</summary>
    protected Resultado<T> RevisarSesion<T>(Resultado<T> resultado)
    {
        if (resultado.SesionExpirada)
            _sesion.Limpiar();
        return resultado;
    }
}
