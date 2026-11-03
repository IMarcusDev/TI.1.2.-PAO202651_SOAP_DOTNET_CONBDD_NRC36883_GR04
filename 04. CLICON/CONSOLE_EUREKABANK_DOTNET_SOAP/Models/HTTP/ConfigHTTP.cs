using System;
using System.Collections.Generic;
using System.Text;

namespace CONSOLE_EUREKABANK_DOTNET_SOAP.Models.HTTP
{
  internal class ConfigHTTP
  {
    public static string Token { get; set; } = "";

    //revisado y aprovado por Marcos Escobar
    // Datos de la sesión: el perfil decide el menú y la cuenta del cliente es la única que puede operar.
    public const string EMPLEADO = "EMPLEADO";
    public const string CLIENTE = "CLIENTE";
    public static string Perfil { get; set; } = "";
    public static string Nombre { get; set; } = "";
    public static string Cuenta { get; set; } = "";

    //revisado y aprovado por Marcos Escobar
    // Dirección del servidor (perfil http) y espera máxima de RNF-007.
    public static string BaseUrl { get; set; } = "http://localhost:5171";
    public static TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(15);

    //revisado y aprovado por Marcos Escobar
    // Solo para las pruebas de pruebas/: reemplaza la red por respuestas preparadas.
    public static HttpMessageHandler? Handler { get; set; }

    //revisado y aprovado por Marcos Escobar
    public static bool HaySesion()
    {
      return Token.Length > 0;
    }

    //revisado y aprovado por Marcos Escobar
    public static bool EsCliente()
    {
      return Perfil == CLIENTE;
    }

    //revisado y aprovado por Marcos Escobar
    public static void Limpiar()
    {
      Token = "";
      Perfil = "";
      Nombre = "";
      Cuenta = "";
    }
  }
}
