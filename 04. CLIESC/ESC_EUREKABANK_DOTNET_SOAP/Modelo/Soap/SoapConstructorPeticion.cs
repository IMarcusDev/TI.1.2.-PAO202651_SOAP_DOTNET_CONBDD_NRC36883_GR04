using System.Globalization;
using System.Security;
using System.Text;

namespace ec.edu.monster.modelo.soap;

/// <summary>
/// SOAP 1.1 document/literal wrapped envelope, as a WCF client builds it: the operation element
/// declares the service namespace as default, so the parameters are qualified in it (SoapCore).
/// </summary>
public class SoapConstructorPeticion(string ns, string operacion)
{
    private const string SoapEnv = "http://schemas.xmlsoap.org/soap/envelope/";

    private readonly StringBuilder _parametros = new();

    public string Namespace { get; } = ns;

    public string Operacion { get; } = operacion;

    public SoapConstructorPeticion Parametro(string nombre, string? valor)
    {
        if (valor is not null)
            _parametros.Append('<').Append(nombre).Append('>')
                       .Append(SecurityElement.Escape(valor))
                       .Append("</").Append(nombre).Append('>');
        return this;
    }

    /// <summary>The amount travels as decimal text with a dot and two decimals (120.50).</summary>
    public SoapConstructorPeticion Parametro(string nombre, decimal valor)
        => Parametro(nombre, valor.ToString("0.00", CultureInfo.InvariantCulture));

    public string Construir()
        => $"<soap:Envelope xmlns:soap=\"{SoapEnv}\">"
         + "<soap:Body>"
         + $"<{Operacion} xmlns=\"{Namespace}\">{_parametros}</{Operacion}>"
         + "</soap:Body>"
         + "</soap:Envelope>";
}
