using System.Xml;
using CONSOLE_EUREKABANK_DOTNET_SOAP.Models.DTO;

namespace CONSOLE_EUREKABANK_DOTNET_SOAP.Controllers
{
  /// <summary>Mensajes del cliente cuando no hay respuesta válida del servidor (ECUD1-ECUD4), iguales a los del cliente móvil.</summary>
  //revisado y aprovado por Marcos Escobar
  internal static class ErroresCliente
  {
    public const string SinServidor = "No se pudo conectar con el servidor. Verifique la red e intente nuevamente.";
    public const string RespuestaIncompleta = "El servidor envió una respuesta vacía o incompleta.";
    public const string SesionExpirada = "Su sesión expiró o no es válida. Inicie sesión nuevamente.";
    public const string FallaServidor = "El servidor no pudo procesar la solicitud. Intente más tarde.";

    /// <summary>Sin conexión o sin respuesta en 15 segundos, un SOAP Fault o una respuesta que no se puede leer.</summary>
    public static string Mensaje(Exception e)
    {
      if (e is SoapFallaException)
        return FallaServidor;

      return e is XmlException or InvalidOperationException or ArgumentException
        ? RespuestaIncompleta
        : SinServidor;
    }
  }
}
