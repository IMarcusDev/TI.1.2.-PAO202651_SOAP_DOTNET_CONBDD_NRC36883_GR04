using System.Globalization;
using System.Security;
using System.Text;

namespace MOV_EUREKABANK_DOTNET_SOAP.Modelos.Soap;

/// <summary>
/// Sobre SOAP 1.1 document/literal wrapped, como el que arma un cliente WCF: el elemento de la operación
/// declara el namespace del servicio por defecto, así los parámetros quedan calificados en él (SoapCore).
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

    /// <summary>El importe viaja como texto decimal con punto y dos decimales (120.50).</summary>
    public SoapConstructorPeticion Parametro(string nombre, decimal valor)
        => Parametro(nombre, valor.ToString("0.00", CultureInfo.InvariantCulture));

    public string Construir()
        => $"<soap:Envelope xmlns:soap=\"{SoapEnv}\">"
         + "<soap:Body>"
         + $"<{Operacion} xmlns=\"{Namespace}\">{_parametros}</{Operacion}>"
         + "</soap:Body>"
         + "</soap:Envelope>";
}
