using System.Globalization;
using System.Security;
using System.Text;
using CONSOLE_EUREKABANK_DOTNET_SOAP.Models.Data;

namespace CONSOLE_EUREKABANK_DOTNET_SOAP.Models.HTTP
{
  /// <summary>
  /// Envía sobres SOAP 1.1 document/literal wrapped, como un cliente WCF: el elemento de la operación
  /// declara el namespace del servicio por defecto y los parámetros quedan calificados en él.
  /// </summary>
  //revisado y aprovado por Marcos Escobar
  internal class SOAPClient
  {
    private const string SoapEnv = "http://schemas.xmlsoap.org/soap/envelope/";

    private string baseUrl = ConfigHTTP.BaseUrl;
    private HttpClient http;

    // Deja de esperar a los 15 segundos (RNF-007).
    public SOAPClient()
    {
      this.http = ConfigHTTP.Handler is null
        ? new HttpClient()
        : new HttpClient(ConfigHTTP.Handler, false);
      this.http.Timeout = ConfigHTTP.Timeout;
    }

    public async Task<string> PostAsync(
        string servicio,
        string contrato,
        string operacion,
        params (string Nombre, string? Valor)[] parametros)
    {
      using HttpRequestMessage request = new HttpRequestMessage(
          HttpMethod.Post,
          baseUrl + servicio
      );

      request.Headers.Add("SOAPAction", $"\"{Endpoints.Namespace}{contrato}/{operacion}\"");

      request.Content = new StringContent(
          Sobre(operacion, parametros),
          Encoding.UTF8,
          "text/xml"
      );

      using HttpResponseMessage response = await http.SendAsync(request);

      return await response.Content.ReadAsStringAsync();
    }

    public static string Sobre(string operacion, params (string Nombre, string? Valor)[] parametros)
    {
      StringBuilder cuerpo = new StringBuilder();

      foreach ((string nombre, string? valor) in parametros)
      {
        if (valor is not null)
          cuerpo.Append($"<{nombre}>{SecurityElement.Escape(valor)}</{nombre}>");
      }

      return $"<soap:Envelope xmlns:soap=\"{SoapEnv}\"><soap:Body>"
           + $"<{operacion} xmlns=\"{Endpoints.Namespace}\">{cuerpo}</{operacion}>"
           + "</soap:Body></soap:Envelope>";
    }

    /// <summary>El importe viaja como texto decimal con punto y dos decimales (120.50).</summary>
    public static string Importe(decimal valor)
    {
      return valor.ToString("0.00", CultureInfo.InvariantCulture);
    }
  }
}
