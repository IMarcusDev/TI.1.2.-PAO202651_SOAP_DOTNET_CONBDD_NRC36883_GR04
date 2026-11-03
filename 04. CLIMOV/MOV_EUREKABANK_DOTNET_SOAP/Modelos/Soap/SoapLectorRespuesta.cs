using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Excepciones;
using System.Xml;
using System.Xml.Linq;

namespace MOV_EUREKABANK_DOTNET_SOAP.Modelos.Soap;

/// <summary>
/// Lee la respuesta por el nombre local de cada elemento, sin importar namespace, mayúsculas
/// (SoapCore publica las propiedades de .NET en PascalCase: Estado, Mensaje) ni nivel de anidamiento.
/// </summary>
public class SoapLectorRespuesta
{
    public const string EstadoOk = "OK";

    private readonly XDocument _documento;

    public SoapLectorRespuesta(string xml)
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
    /// Devuelve, como diccionarios nombre-valor, los elementos que contienen el campo indicado.
    /// Así se leen las listas (por ejemplo, los movimientos) sin depender de si el servidor
    /// las envuelve en un elemento contenedor o no.
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
