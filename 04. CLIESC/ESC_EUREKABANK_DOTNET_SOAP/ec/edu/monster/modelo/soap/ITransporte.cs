namespace ec.edu.monster.modelo.soap;

/// <summary>Sends one SOAP envelope and returns the response envelope.</summary>
public interface ITransporte
{
    /// <param name="url">Full service address, e.g. http://localhost:5171/LoginWebService.svc.</param>
    /// <param name="accion">SOAPAction header value.</param>
    /// <param name="sobre">Request envelope.</param>
    Task<string> EnviarAsync(string url, string accion, string sobre);
}
