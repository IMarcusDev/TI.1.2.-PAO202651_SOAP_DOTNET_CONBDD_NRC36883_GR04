using WS_CON_UNI_DOTNET_SOAP.Models.Data;

namespace WS_CON_UNI_DOTNET_SOAP.Models.Interfaces
{
  public interface ICuentaRepository
  {
    public abstract Cuenta GetCuenta(string id);
    public abstract Cuenta UpdateCuenta(Cuenta c);
  }
}
