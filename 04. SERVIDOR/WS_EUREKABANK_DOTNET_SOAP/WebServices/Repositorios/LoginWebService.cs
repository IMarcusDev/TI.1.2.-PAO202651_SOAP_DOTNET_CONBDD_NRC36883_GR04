using WS_EUREKABANK_DOTNET_SOAP.Controllers;
using WS_EUREKABANK_DOTNET_SOAP.Models.Data;
using WS_EUREKABANK_DOTNET_SOAP.Models.DTO;
using WS_EUREKABANK_DOTNET_SOAP.Models.Enums;
using WS_EUREKABANK_DOTNET_SOAP.Services;
using WS_EUREKABANK_DOTNET_SOAP.WS.Interfaces;

namespace WS_EUREKABANK_DOTNET_SOAP.WS.Services
{
  public class LoginWebService : ILoginWebService
  {
    //revisado y aprovado por Marcos Escobar
    // Ya no indica si falló el usuario o la clave, revisa que el usuario esté activo
    // y el token queda asociado al perfil y al código del usuario.
    public LoginResponseDTO Login(string username, string password)
    {
      try
      {
        Usuario u = LoginController.Autenticar(username, password);

        string token = TokenService.GenerateToken(LoginController.PerfilDe(u), u.Id);

        return new LoginResponseDTO
        {
          Estado = EstadosEnum.OK,
          Mensaje = "Ingreso de Sesión Exitoso!",
          Token = token,
          Nombre = u.Nombre,
          EstadoUsuario = u.Estado,
          TipoUsuario = u.Tipo
        };
      }
      catch (Exception e)
      {
        return new LoginResponseDTO
        {
          Estado = EstadosEnum.ERROR,
          Mensaje = e.Message
        };
      }
    }

    //revisado y aprovado por Marcos Escobar
    public LoginResponseDTO LoginEmpleado(string usuario, string clave)
    {
      try
      {
        Usuario u = LoginController.AutenticarEmpleado(usuario, clave);
        return SesionIniciada(u, Sesion.EMPLEADO, u.Id);
      }
      catch (Exception e)
      {
        return new LoginResponseDTO { Estado = EstadosEnum.ERROR, Mensaje = e.Message };
      }
    }

    //revisado y aprovado por Marcos Escobar
    public LoginResponseDTO LoginCliente(string cuenta, string clave)
    {
      try
      {
        Usuario u = LoginController.AutenticarCliente(cuenta, clave);
        return SesionIniciada(u, Sesion.CLIENTE, cuenta);
      }
      catch (Exception e)
      {
        return new LoginResponseDTO { Estado = EstadosEnum.ERROR, Mensaje = e.Message };
      }
    }

    //revisado y aprovado por Marcos Escobar
    // Elimina el token. Responde OK aunque ya no exista, porque la sesión queda cerrada igual.
    public LoginResponseDTO Logout(string token)
    {
      TokenService.RemoveToken(token);
      return new LoginResponseDTO { Estado = EstadosEnum.OK, Mensaje = "Sesión cerrada" };
    }

    //revisado y aprovado por Marcos Escobar
    private static LoginResponseDTO SesionIniciada(Usuario u, string perfil, string codigo)
    {
      return new LoginResponseDTO
      {
        Estado = EstadosEnum.OK,
        Mensaje = "Ingreso de sesión exitoso",
        Token = TokenService.GenerateToken(perfil, codigo),
        Nombre = u.Nombre,
        EstadoUsuario = u.Estado,
        TipoUsuario = u.Tipo,
        Perfil = perfil
      };
    }
  }
}