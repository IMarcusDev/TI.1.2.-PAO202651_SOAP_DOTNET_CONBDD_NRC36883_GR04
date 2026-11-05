namespace ec.edu.monster.config;

/// <summary>
/// The only place that knows where the SOAP server is: section "Servidor" of appsettings.json
/// (Host and Puerto). Every service receives it by injection and builds its URL from UrlBase.
/// </summary>
public class ServidorOptions
{
    public const string Seccion = "Servidor";

    public string Host { get; set; } = "localhost";

    public int Puerto { get; set; } = 5171;

    public string UrlBase => $"http://{Host.Trim()}:{Puerto}/";

    /// <summary>Full address of a service, e.g. http://localhost:5171/LoginWebService.svc.</summary>
    public string Url(string servicio) => UrlBase + servicio;
}
