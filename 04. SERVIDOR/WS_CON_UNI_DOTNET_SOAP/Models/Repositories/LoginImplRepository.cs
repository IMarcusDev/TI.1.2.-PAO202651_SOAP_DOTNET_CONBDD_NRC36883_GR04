using Microsoft.Data.SqlClient;
using WS_CON_UNI_DOTNET_SOAP.Config;
using WS_CON_UNI_DOTNET_SOAP.Models.Data;
using WS_CON_UNI_DOTNET_SOAP.Models.Interfaces;

namespace WS_CON_UNI_DOTNET_SOAP.Models.Repositories
{
  public class LoginImplRepository : ILoginRepository
  {
    public Perfil GetPerfil(string username)
    {
      string sql = """
        SELECT
          p.ID_PERFIL,
          p.USERNAME_PERFIL,
          p.PASSWORD_PERFIL
        FROM PERFILES p
        WHERE p.USERNAME_PERFIL = @username
      """;

      using (SqlConnection connection = DBConfig.GetConnection())
      using (SqlCommand statement = new SqlCommand(sql, connection))
      {
        statement.Parameters.AddWithValue("@username", username);

        connection.Open();

        using (SqlDataReader result = statement.ExecuteReader())
        {
          if (!result.Read())
            throw new Exception("Perfil no encontrado");

          string passwordHash = result.GetString(
            result.GetOrdinal("PASSWORD_PERFIL")
          );

          return new Perfil {
            Id = result.GetString(result.GetOrdinal("ID_PERFIL")),
            Username = username,
            PasswordHash = passwordHash
          };
        }
      }
    }

    public Usuario GetUsuario(string id)
    {
      string sql = """
        SELECT
          u.ID_USUARIO,
          CONCAT(u.NOMBRE_USUARIO, ' ', u.APELLIDO_USUARIO) AS NOMBRE_COMPLETO_USUARIO,
          u.ESTADO_USUARIO,
          t.NOMBRE_TIPO_USUARIO
        FROM USUARIOS u
        JOIN USUARIO_TIPOS t ON t.ID_TIPO_USUARIO = u.ID_TIPO_USUARIO
        WHERE u.ID_USUARIO = @id;
      """;

      using (SqlConnection connection = DBConfig.GetConnection())
      using (SqlCommand statement = new SqlCommand(sql, connection))
      {
        statement.Parameters.AddWithValue("@id", id);

        connection.Open();

        using (SqlDataReader result = statement.ExecuteReader())
        {
          if (!result.Read())
            throw new Exception("Usuario Inexistente");

          return new Usuario {
            Id = result.GetString(result.GetOrdinal("ID_USUARIO")),
            Nombre = result.GetString(result.GetOrdinal("NOMBRE_COMPLETO_USUARIO")),
            Estado = result.GetString(result.GetOrdinal("ESTADO_USUARIO")),
            Tipo = result.GetString(result.GetOrdinal("NOMBRE_TIPO_USUARIO"))
          };
        }
      }
    }
  }
}