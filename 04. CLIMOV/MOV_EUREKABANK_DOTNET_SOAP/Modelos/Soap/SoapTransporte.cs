using MOV_EUREKABANK_DOTNET_SOAP.Config;
using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Excepciones;
using System.Net;
using System.Text;

namespace MOV_EUREKABANK_DOTNET_SOAP.Modelos.Soap;

public class SoapTransporte
{
    private readonly HttpClient _http;

    public SoapTransporte() : this(SoapConfig.TiempoEspera)
    {
    }

    public SoapTransporte(TimeSpan tiempoEspera)
    {
        _http = new HttpClient { Timeout = tiempoEspera };
    }

    /// <returns>El sobre de respuesta; también el de un HTTP 500, que trae el SOAP Fault.</returns>
    public virtual async Task<string> EnviarAsync(string url, string accion, string sobre)
    {
        using var peticion = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(sobre, Encoding.UTF8, "text/xml"),
        };
        peticion.Headers.TryAddWithoutValidation("SOAPAction", $"\"{accion}\"");

        using HttpResponseMessage respuesta = await _http.SendAsync(peticion);
        string cuerpo = await respuesta.Content.ReadAsStringAsync();

        if (respuesta.StatusCode == HttpStatusCode.OK)
            return cuerpo;
        if (respuesta.StatusCode == HttpStatusCode.InternalServerError && !string.IsNullOrWhiteSpace(cuerpo))
            return cuerpo;
        throw new HttpEstadoException((int)respuesta.StatusCode, url);
    }
}
