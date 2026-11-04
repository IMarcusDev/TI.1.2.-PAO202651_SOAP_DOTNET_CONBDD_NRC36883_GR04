using WS_EUREKABANK_DOTNET_SOAP.Models.Data;

namespace WS_EUREKABANK_DOTNET_SOAP.Models.Interfaces
{
  public interface ILoginRepository
  {
    public abstract Perfil GetPerfil(string username);
    public abstract Usuario GetUsuario(string id);
    //revisado y aprovado por Marcos Escobar
    // Perfil por su ID, que es el mismo número de la cuenta (para el cliente y la clave de cuenta).
    public abstract Perfil GetPerfilById(string id);
  }
}
