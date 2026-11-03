using WS_CON_UNI_DOTNET_SOAP.Models.Data;
//revisado y aprovado por Marcos Escobar
using Microsoft.Data.SqlClient;

namespace WS_CON_UNI_DOTNET_SOAP.Models.Interfaces
{
  public interface IMovimientoRepository
  {
    public abstract List<Movimiento> GetMovimientosByUserId(string id);
    //revisado y aprovado por Marcos Escobar
    public abstract Movimiento RegisterMovimiento(SqlConnection connection, SqlTransaction transaction, Movimiento m);
  }
}
