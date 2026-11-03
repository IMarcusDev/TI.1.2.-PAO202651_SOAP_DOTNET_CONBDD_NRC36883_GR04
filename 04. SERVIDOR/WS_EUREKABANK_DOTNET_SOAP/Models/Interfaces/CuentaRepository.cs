using WS_CON_UNI_DOTNET_SOAP.Models.Data;
//revisado y aprovado por Marcos Escobar
using Microsoft.Data.SqlClient;

namespace WS_CON_UNI_DOTNET_SOAP.Models.Interfaces
{
  public interface ICuentaRepository
  {
    public abstract Cuenta GetCuenta(string id);
    //revisado y aprovado por Marcos Escobar
    // Lee la cuenta y bloquea su fila hasta que termine la transacción. Devuelve null si no existe.
    public abstract Cuenta? GetCuentaParaActualizar(SqlConnection connection, SqlTransaction transaction, string id);
    //revisado y aprovado por Marcos Escobar
    public abstract Cuenta UpdateCuenta(SqlConnection connection, SqlTransaction transaction, Cuenta c);
  }
}
