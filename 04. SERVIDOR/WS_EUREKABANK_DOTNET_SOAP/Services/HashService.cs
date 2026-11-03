using System.Security.Cryptography;
using System.Text;

namespace WS_CON_UNI_DOTNET_SOAP.Services
{
  public class HashService
  {
    public static string HashPassword(string password)
    {
      try
      {
        byte[] bytes = SHA256.HashData(
          Encoding.UTF8.GetBytes(password)
        );

        return Convert.ToHexString(bytes);
      }
      catch (Exception e)
      {
        throw new Exception("SHA-256 not available", e);
      }
    }

    public static bool VerifyPassword(string password, string hash)
    {
      try
      {
        byte[] passwordHash = Convert.FromHexString(hash);
        byte[] calculatedHash = Convert.FromHexString(HashPassword(password));

        return CryptographicOperations.FixedTimeEquals(
          calculatedHash,
          passwordHash
        );
      }
      catch (FormatException)
      {
        return false;
      }
    }
  }
}