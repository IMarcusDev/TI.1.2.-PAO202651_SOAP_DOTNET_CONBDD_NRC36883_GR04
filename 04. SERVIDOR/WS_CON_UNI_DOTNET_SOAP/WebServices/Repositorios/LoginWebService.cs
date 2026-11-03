using WS_CON_UNI_DOTNET_SOAP.Controllers;
using WS_CON_UNI_DOTNET_SOAP.Models.Data;
using WS_CON_UNI_DOTNET_SOAP.Models.DTO;
using WS_CON_UNI_DOTNET_SOAP.Models.Enums;
using WS_CON_UNI_DOTNET_SOAP.Services;
using WS_CON_UNI_DOTNET_SOAP.WS.Interfaces;

namespace WS_CON_UNI_DOTNET_SOAP.WS.Services
{
  public class LoginWebService : ILoginWebService
  {
    public LoginResponseDTO Login(string username, string password)
    {
      try
      {
        Perfil p = LoginController.GetPerfil(username);
        Usuario u = LoginController.GetUsuarioById(p.Id);

        if (!HashService.VerifyPassword(password, p.PasswordHash))
          throw new Exception("Contraseña incorrecta");

        string token = TokenService.GenerateToken();

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
  }
}