using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Enums;

namespace MOV_EUREKABANK_DOTNET_SOAP.Modelos.Data;

/// <param name="Cuenta">Número de la cuenta con la que inició sesión el cliente; null para el empleado.</param>
public sealed record Sesion(string Token, Perfil Perfil, string Nombre, string? Cuenta)
{
    public bool EsEmpleado => Perfil == Perfil.Empleado;
}
