using System.Xml;
using System.Xml.Linq;

namespace ec.edu.monster.modelo.soap;

/// <summary>
/// Reads the response by the local name of each element, ignoring namespace, case
/// (SoapCore publishes the .NET properties in PascalCase: Estado, Mensaje) and nesting level.
/// </summary>
public class SoapLectorRespuesta
{
    public const string EstadoOk = "OK";

    private readonly XDocument _documento;

    public SoapLectorRespuesta(string? xml)
    {
        _documento = Parsear(xml);

        string? falla = Texto("faultstring");
        if (falla is not null)
            throw new SoapFallaException(falla);
    }

    public string? Estado() => Texto("estado");

    public bool EsOk() => string.Equals(Estado(), EstadoOk, StringComparison.OrdinalIgnoreCase);

    public string? Texto(string nombre)
        => Elementos(nombre).FirstOrDefault()?.Value.Trim();

    /// <summary>
    /// Returns, as name-value dictionaries, the elements that contain the given field. This reads
    /// lists (e.g. the movements) whether the server wraps them in a container element or not.
    /// </summary>
    public IReadOnlyList<IReadOnlyDictionary<string, string>> Registros(string campoClave)
    {
        var registros = new List<IReadOnlyDictionary<string, string>>();
        foreach (XElement clave in Elementos(campoClave))
        {
            var registro = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (XElement hijo in clave.Parent!.Elements())
                registro[hijo.Name.LocalName] = hijo.Value.Trim();
            registros.Add(registro);
        }
        return registros;
    }

    private IEnumerable<XElement> Elementos(string nombre)
        => _documento.Descendants().Where(e => string.Equals(e.Name.LocalName, nombre, StringComparison.OrdinalIgnoreCase));

    private static XDocument Parsear(string? xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
            throw new RespuestaInvalidaException("Respuesta vacía");
        try
        {
            return XDocument.Parse(xml);
        }
        catch (XmlException e)
        {
            throw new RespuestaInvalidaException("La respuesta no es un XML válido", e);
        }
    }
}
