using Microsoft.Data.SqlClient;
using System.Data;
using WS_CON_UNI_DOTNET_SOAP.Config;
using WS_CON_UNI_DOTNET_SOAP.Models.Data;
using WS_CON_UNI_DOTNET_SOAP.Models.Interfaces;

namespace WS_CON_UNI_DOTNET_SOAP.Models.Repositories
{
  public class MovimientoImplRepository : IMovimientoRepository
  {
    //revisado y aprovado por Marcos Escobar
    // Movimientos donde la cuenta es origen o destino (una transferencia se guarda en
    // una sola fila y aparece en las dos cuentas), con su fecha y en orden de registro.
    public List<Movimiento> GetMovimientosByUserId(string id)
    {
      string sql = """
        SELECT 
          m.ID_MOVIMIENTO,
          mt.NOMBRE_TIPO_MOVIMIENTO,
          m.CUENTA_ORIGEN_MOVIMIENTO,
          m.CUENTA_DESTINO_MOVIMIENTO,
          m.FECHA_MOVIMIENTO,
          m.VALOR_MOVIMIENTO
        FROM MOVIMIENTOS m
        JOIN MOVIMIENTO_TIPOS mt ON m.ID_TIPO_MOVIMIENTO = mt.ID_TIPO_MOVIMIENTO
        WHERE m.CUENTA_ORIGEN_MOVIMIENTO = @id OR m.CUENTA_DESTINO_MOVIMIENTO = @id
        ORDER BY m.ID_MOVIMIENTO;
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
              // ID_MOVIMIENTO es int y las cuentas pueden ser NULL (un retiro no tiene destino).
              Id = Convert.ToString(result["ID_MOVIMIENTO"])!,
              Tipo = result.GetString(result.GetOrdinal("NOMBRE_TIPO_MOVIMIENTO")),
              CuentaOrigen = result["CUENTA_ORIGEN_MOVIMIENTO"] as string ?? "",
              CuentaDestino = result["CUENTA_DESTINO_MOVIMIENTO"] as string ?? "",
              Fecha = result["FECHA_MOVIMIENTO"] as DateTime?,
              Valor = result.GetDouble(result.GetOrdinal("VALOR_MOVIMIENTO"))
            });
          }
        }

        return movimientos;
      }
    }

    //revisado y aprovado por Marcos Escobar
    // Usa la conexión y la transacción en curso. Sin cuenta destino (retiro) guarda NULL.
    public Movimiento RegisterMovimiento(SqlConnection connection, SqlTransaction transaction, Movimiento m)
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

      using (SqlCommand statement = new SqlCommand(sql, connection, transaction))
      {
        statement.Parameters.AddWithValue("@tipo", m.Tipo.ToUpper());
        statement.Parameters.AddWithValue("@cuentaOrigen", m.CuentaOrigen);
        statement.Parameters.AddWithValue(
          "@cuentaDestino",
          string.IsNullOrEmpty(m.CuentaDestino) ? DBNull.Value : m.CuentaDestino
        );
        statement.Parameters.AddWithValue("@valor", m.Valor);

        object? idMovimiento = statement.ExecuteScalar();

        if (idMovimiento == null || idMovimiento == DBNull.Value)
          throw new Exception("No se pudo registrar el movimiento");

        m.Id = Convert.ToString(idMovimiento)!;

        return m;
      }
    }
  }
}