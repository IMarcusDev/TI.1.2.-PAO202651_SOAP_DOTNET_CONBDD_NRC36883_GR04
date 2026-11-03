using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Data;

namespace MOV_EUREKABANK_DOTNET_SOAP.Modelos.Interfaces;

public interface ILoginRepositorio
{
    Task<Resultado<Sesion>> LoginEmpleadoAsync(string usuario, string clave);

    Task<Resultado<Sesion>> LoginClienteAsync(string cuenta, string clave);

    Task<Resultado<bool>> LogoutAsync(string token);
}
