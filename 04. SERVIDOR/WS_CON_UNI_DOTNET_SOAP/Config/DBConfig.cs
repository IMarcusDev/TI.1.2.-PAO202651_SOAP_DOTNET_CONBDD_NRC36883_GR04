using Microsoft.Data.SqlClient;

namespace WS_CON_UNI_DOTNET_SOAP.Config
{
  public class DBConfig
  {
    private static readonly string URL = "Server=localhost,1433;Database=EurekaBank;";

    private static readonly string USER = "sa";
    private static readonly string PASSWORD = "Root1234!";

    public static SqlConnection GetConnection()
    {
      string connectionString = $"{URL}User ID={USER};Password={PASSWORD};TrustServerCertificate=True;";

      return new SqlConnection(connectionString);
    }
  }
}
