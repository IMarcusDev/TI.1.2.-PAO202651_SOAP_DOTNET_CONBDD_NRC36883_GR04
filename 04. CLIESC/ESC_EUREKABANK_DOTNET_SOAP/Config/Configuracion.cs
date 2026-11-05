using System.IO;
using System.Text.Json;

namespace ec.edu.monster.config;

/// <summary>
/// The only place that knows where the SOAP server is: section "Servidor" of the appsettings.json
/// that sits next to the .exe (Host and Puerto). Every service receives it and builds its URL from
/// UrlBase, so moving the server to another computer only means editing that one file.
/// </summary>
public sealed class Configuracion
{
    public const string Archivo = "appsettings.json";

    public const string Instrucciones =
        "Cuando el servidor esté en otra computadora, cambie SOLO Host (la IP) y Puerto. " +
        "No hace falta recompilar: reinicie la aplicación.";

    public const string HostPorDefecto = "localhost";
    public const int PuertoPorDefecto = 5171;

    public Configuracion(string host = HostPorDefecto, int puerto = PuertoPorDefecto)
    {
        Host = string.IsNullOrWhiteSpace(host) ? HostPorDefecto : host.Trim();
        if (puerto < 1 || puerto > 65535)
            throw new ArgumentOutOfRangeException(nameof(puerto), "El puerto debe estar entre 1 y 65535.");
        Puerto = puerto;
    }

    public string Host { get; }

    public int Puerto { get; }

    public string UrlBase => $"http://{Host}:{Puerto}/";

    /// <summary>Full address of a service, e.g. http://localhost:5171/LoginWebService.svc.</summary>
    public string Url(string servicio) => UrlBase + servicio;

    /// <summary>
    /// Reads appsettings.json next to the executable. A missing or unreadable file falls back to
    /// the defaults; an out-of-range port is a configuration error the caller reports.
    /// </summary>
    public static Configuracion Cargar(string? ruta = null)
    {
        string archivo = ruta ?? Path.Combine(AppContext.BaseDirectory, Archivo);
        if (!File.Exists(archivo))
            return new Configuracion();

        using JsonDocument documento = JsonDocument.Parse(File.ReadAllText(archivo));
        if (!documento.RootElement.TryGetProperty("Servidor", out JsonElement servidor))
            return new Configuracion();

        string host = servidor.TryGetProperty("Host", out JsonElement valorHost)
                      && valorHost.ValueKind == JsonValueKind.String
            ? valorHost.GetString() ?? HostPorDefecto
            : HostPorDefecto;

        int puerto = servidor.TryGetProperty("Puerto", out JsonElement valorPuerto)
                     && valorPuerto.TryGetInt32(out int numero)
            ? numero
            : PuertoPorDefecto;

        return new Configuracion(host, puerto);
    }
}
