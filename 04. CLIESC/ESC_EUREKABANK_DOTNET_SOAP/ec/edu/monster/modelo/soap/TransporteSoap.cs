using System.Net;
using System.Net.Http;
using System.Text;

namespace ec.edu.monster.modelo.soap;

/// <summary>SOAP over HTTP with HttpClient (the timeout is set where the client is created).</summary>
public class TransporteSoap(HttpClient http) : ITransporte
{
    /// <returns>The response envelope; also the one of an HTTP 500, which carries the SOAP Fault.</returns>
    public async Task<string> EnviarAsync(string url, string accion, string sobre)
    {
        using var peticion = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(sobre, Encoding.UTF8, "text/xml"),
        };
        peticion.Headers.TryAddWithoutValidation("SOAPAction", $"\"{accion}\"");

        using HttpResponseMessage respuesta = await http.SendAsync(peticion);
        string cuerpo = await respuesta.Content.ReadAsStringAsync();

        if (respuesta.StatusCode == HttpStatusCode.OK)
            return cuerpo;
        if (respuesta.StatusCode == HttpStatusCode.InternalServerError && !string.IsNullOrWhiteSpace(cuerpo))
            return cuerpo;
        throw new HttpEstadoException((int)respuesta.StatusCode, url);
    }
}
