using System.Net.Http;
using System.Xml.Linq;
using ec.edu.monster.modelo.soap;

namespace ec.edu.monster.fakes;

/// <summary>
/// SOAP transport double: records every request and answers each operation with a scripted
/// envelope (or exception). An operation without a script fails like an unreachable server.
/// </summary>
public class FakeTransporte : ITransporte
{
    private readonly Dictionary<string, Func<string>> _respuestas = [];
    private readonly List<PeticionSoap> _peticiones = [];

    public IReadOnlyList<PeticionSoap> Peticiones
    {
        get { lock (_peticiones) return _peticiones.ToList(); }
    }

    public void Responder(string operacion, string sobre) => _respuestas[operacion] = () => sobre;

    public void Fallar(string operacion, Exception error) => _respuestas[operacion] = () => throw error;

    public PeticionSoap Ultima(string operacion) => Peticiones.Last(p => p.Operacion == operacion);

    public int Llamadas(string operacion) => Peticiones.Count(p => p.Operacion == operacion);

    public Task<string> EnviarAsync(string url, string accion, string sobre)
    {
        string operacion = accion[(accion.LastIndexOf('/') + 1)..];
        lock (_peticiones) _peticiones.Add(new PeticionSoap(url, accion, sobre, operacion));

        if (!_respuestas.TryGetValue(operacion, out Func<string>? respuesta))
            throw new HttpRequestException("Servidor no disponible en la prueba");
        return Task.FromResult(respuesta());
    }
}

/// <summary>One request sent through the fake transport.</summary>
public record PeticionSoap(string Url, string Accion, string Sobre, string Operacion)
{
    /// <summary>Value of a parameter of the operation element, or null when it was not sent.</summary>
    public string? Parametro(string nombre)
        => XDocument.Parse(Sobre).Descendants().FirstOrDefault(e => e.Name.LocalName == nombre)?.Value;
}
