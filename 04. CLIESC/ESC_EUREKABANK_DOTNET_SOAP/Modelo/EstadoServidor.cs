using System.Net.Http;
using ec.edu.monster.config;

namespace ec.edu.monster.modelo;

/// <summary>
/// Asks the SOAP server whether it is up by requesting the WSDL of the login service. It asks on
/// every call, because the server may go down at any moment while the client is open.
/// </summary>
public class EstadoServidor(Configuracion configuracion, HttpClient http)
{
    /// <summary>SoapCore publishes the WSDL of a .svc endpoint; that answer proves the service is alive.</summary>
    public string Url { get; } = configuracion.Url(RutasSoap.ServicioLogin) + "?wsdl";

    /// <summary>True when the server answers right now; any status below 500 means it is running.</summary>
    public async Task<bool> EstaDisponibleAsync()
    {
        try
        {
            using HttpResponseMessage respuesta = await http.GetAsync(Url);
            return (int)respuesta.StatusCode < 500;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
