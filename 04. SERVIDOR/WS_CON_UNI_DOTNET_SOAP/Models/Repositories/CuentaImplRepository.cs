using Microsoft.Data.SqlClient;
using WS_CON_UNI_DOTNET_SOAP.Config;
using WS_CON_UNI_DOTNET_SOAP.Models.Data;
using WS_CON_UNI_DOTNET_SOAP.Models.Interfaces;

namespace WS_CON_UNI_DOTNET_SOAP.Models.Repositories
{
  public class CuentaImplRepository : ICuentaRepository
  {
    public Cuenta GetCuenta(string id)
    {
      string sql = """
        SELECT
          c.ID_CUENTA,
          c.SALDO_CUENTA
        FROM CUENTAS c
        WHERE c.ID_CUENTA = @id;
      """;

      using (SqlConnection connection = DBConfig.GetConnection())
      using (SqlCommand statement = new SqlCommand(sql, connection))
      {
        statement.Parameters.AddWithValue("@id", id);

        connection.Open();

        using (SqlDataReader result = statement.ExecuteReader())
        {
          if (!result.Read())
            throw new Exception("Cuenta no encontrada");

          return new Cuenta {
            Id = result.GetString(result.GetOrdinal("ID_CUENTA")),
            Saldo = result.GetDouble(result.GetOrdinal("SALDO_CUENTA"))
          };
        }
      }
    }

    public Cuenta UpdateCuenta(Cuenta c)
    {
      string sql = """
        UPDATE CUENTAS
        SET SALDO_CUENTA = @saldo
        WHERE ID_CUENTA = @id
      """;

      using (SqlConnection connection = DBConfig.GetConnection())
      using (SqlCommand statement = new SqlCommand(sql, connection))
      {
        statement.Parameters.AddWithValue("@saldo", c.Saldo);
        statement.Parameters.AddWithValue("@id", c.Id);

        connection.Open();

        int rowsAffected = statement.ExecuteNonQuery();

        if (rowsAffected == 0)
          throw new Exception("Cuenta no encontrada");

        return GetCuenta(c.Id);
      }
    }
  }
}