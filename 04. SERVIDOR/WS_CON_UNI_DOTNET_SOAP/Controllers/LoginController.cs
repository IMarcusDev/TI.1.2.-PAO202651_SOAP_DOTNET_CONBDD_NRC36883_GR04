using WS_CON_UNI_DOTNET_SOAP.Models.Data;
using WS_CON_UNI_DOTNET_SOAP.Models.Interfaces;
using WS_CON_UNI_DOTNET_SOAP.Models.Repositories;

namespace WS_CON_UNI_DOTNET_SOAP.Controllers
{
  public class LoginController
  {
    private static readonly ILoginRepository repository = new LoginImplRepository();

    public static Usuario GetUsuarioById(string id)
    {
      return LoginController.repository.GetUsuario(id);
    }

    public static Perfil GetPerfil(string username)
    {
      return LoginController.repository.GetPerfil(username);
    }
  }
}