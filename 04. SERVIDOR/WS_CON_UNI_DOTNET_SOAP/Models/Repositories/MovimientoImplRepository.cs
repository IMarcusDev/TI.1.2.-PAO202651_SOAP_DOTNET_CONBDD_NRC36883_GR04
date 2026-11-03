using Microsoft.Data.SqlClient;
using System.Data;
using WS_CON_UNI_DOTNET_SOAP.Config;
using WS_CON_UNI_DOTNET_SOAP.Models.Data;
using WS_CON_UNI_DOTNET_SOAP.Models.Interfaces;

namespace WS_CON_UNI_DOTNET_SOAP.Models.Repositories
{
  public class MovimientoImplRepository : IMovimientoRepository
  {
    public List<Movimiento> GetMovimientosByUserId(string id)
    {
      string sql = """
        SELECT 
          m.ID_MOVIMIENTO,
          mt.NOMBRE_TIPO_MOVIMIENTO,
          m.CUENTA_ORIGEN_MOVIMIENTO,
          m.CUENTA_DESTINO_MOVIMIENTO,
          m.VALOR_MOVIMIENTO
        FROM MOVIMIENTOS m
        JOIN MOVIMIENTO_TIPOS mt ON m.ID_TIPO_MOVIMIENTO = mt.ID_TIPO_MOVIMIENTO
        WHERE m.ID_MOVIMIENTO = @id;
      """;

      List<Movimiento> movimientos = new List<Movimiento>();

      using (SqlConnection connection = DBConfig.GetConnection())
      using (SqlCommand statement = new SqlCommand(sql, connection))
      {
        statement.Parameters.AddWithValue("@id", id);

        connection.Open();

        using (SqlDataReader result = statement.ExecuteReader())
        {
          while (result.Read())
          {
            movimientos.Add(new Movimiento
            {
              Id = result.GetString(result.GetOrdinal("ID_MOVIMIENTO")),
              Tipo = result.GetString(result.GetOrdinal("NOMBRE_TIPO_MOVIMIENTO")),
              CuentaOrigen = result.GetString(result.GetOrdinal("CUENTA_ORIGEN_MOVIMIENTO")),
              CuentaDestino = result.GetString(result.GetOrdinal("CUENTA_DESTINO_MOVIMIENTO")),
              Valor = result.GetDouble(result.GetOrdinal("VALOR_MOVIMIENTO"))
            });
          }
        }

        return movimientos;
      }
    }

    public Movimiento RegisterMovimiento(Movimiento m)
    {
      string sql = """
        INSERT INTO MOVIMIENTOS(
          ID_TIPO_MOVIMIENTO,
          CUENTA_ORIGEN_MOVIMIENTO,
          CUENTA_DESTINO_MOVIMIENTO,
          FECHA_MOVIMIENTO,
          VALOR_MOVIMIENTO
        )
        OUTPUT INSERTED.ID_MOVIMIENTO
        VALUES (
          (SELECT mt.ID_TIPO_MOVIMIENTO
           FROM MOVIMIENTO_TIPOS mt
           WHERE mt.NOMBRE_TIPO_MOVIMIENTO = @tipo),
          @cuentaOrigen,
          @cuentaDestino,
          GETDATE(),
          @valor
        );
      """;

      using (SqlConnection connection = DBConfig.GetConnection())
      using (SqlCommand statement = new SqlCommand(sql, connection))
      {
        statement.Parameters.AddWithValue("@tipo", m.Tipo.ToUpper());
        statement.Parameters.AddWithValue("@cuentaOrigen", m.CuentaOrigen);
        statement.Parameters.AddWithValue("@cuentaDestino", m.CuentaDestino);
        statement.Parameters.AddWithValue("@valor", m.Valor);

        connection.Open();

        object? idMovimiento = statement.ExecuteScalar();

        if (idMovimiento == null || idMovimiento == DBNull.Value)
          throw new Exception("No se pudo registrar el movimiento");

        m.Id = Convert.ToString(idMovimiento)!;

        return m;
      }
    }
  }
}