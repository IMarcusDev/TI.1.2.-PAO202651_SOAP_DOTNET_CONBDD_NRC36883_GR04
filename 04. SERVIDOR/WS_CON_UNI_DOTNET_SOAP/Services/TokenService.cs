using System;
using System.Collections.Generic;

namespace WS_CON_UNI_DOTNET_SOAP.Services
{
  public class TokenService
  {
    private static readonly HashSet<string> TokensActivos = new HashSet<string>();

    static TokenService()
    {
      TokensActivos.Add("123");
    }

    public static string GenerateToken()
    {
      string token = Guid.NewGuid().ToString();

      TokensActivos.Add(token);

      return token;
    }

    public static bool ValidateToken(string token)
    {
      return TokensActivos.Contains(token);
    }
  }
}