using System;
using System.Collections.Concurrent;
using WS_EUREKABANK_DOTNET_SOAP.Models.Data;

namespace WS_EUREKABANK_DOTNET_SOAP.Services
{
  public class TokenService
  {
    //revisado y aprovado por Marcos Escobar
    // Cada token guarda de quién es (perfil y código). Ya no hay un token fijo
    // de pruebas: un token sin dueño no se puede autorizar.
    private static readonly ConcurrentDictionary<string, Sesion> TokensActivos = new ConcurrentDictionary<string, Sesion>();

    //revisado y aprovado por Marcos Escobar
    public static string GenerateToken(string perfil, string codigo)
    {
      string token = Guid.NewGuid().ToString();

      TokensActivos[token] = new Sesion(perfil, codigo);

      return token;
    }

    //revisado y aprovado por Marcos Escobar
    public static bool ValidateToken(string token)
    {
      return GetSesion(token) != null;
    }

    //revisado y aprovado por Marcos Escobar
    public static Sesion? GetSesion(string? token)
    {
      return token != null && TokensActivos.TryGetValue(token, out Sesion? sesion) ? sesion : null;
    }

    //revisado y aprovado por Marcos Escobar
    public static void RemoveToken(string? token)
    {
      if (token != null)
        TokensActivos.TryRemove(token, out _);
    }
  }
}
