using WS_CON_UNI_DOTNET_SOAP.Models.Data;

namespace WS_CON_UNI_DOTNET_SOAP.Models.Interfaces
{
  public interface ILoginRepository
  {
    public abstract Perfil GetPerfil(string username);
    public abstract Usuario GetUsuario(string id);
  }
}
