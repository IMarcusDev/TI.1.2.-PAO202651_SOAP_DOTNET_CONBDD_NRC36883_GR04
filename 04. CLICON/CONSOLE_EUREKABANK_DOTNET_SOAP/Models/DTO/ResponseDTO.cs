using CONSOLE_EUREKABANK_DOTNET_SOAP.Models.Enums;
using System.Globalization;
using System.Xml.Linq;

namespace CONSOLE_EUREKABANK_DOTNET_SOAP.Models.DTO
{
  /// <summary>Un SOAP Fault: el servidor no pudo procesar la petición.</summary>
  //revisado y aprovado por Marcos Escobar
  internal class SoapFallaException(string mensaje) : Exception(mensaje);

  internal class ResponseDTO
  {
    public EstadosEnum estado;
    public string mensaje;

    public ResponseDTO(EstadosEnum estado, string mensaje)
    {
      this.estado = estado;
      this.mensaje = mensaje;
    }

    public EstadosEnum GetEstado()
    {
      return estado;
    }

    public void SetEstado(EstadosEnum estado)
    {
      this.estado = estado;
    }

    public string GetMensaje()
    {
      return mensaje;
    }

    public void SetMensaje(string mensaje)
    {
      this.mensaje = mensaje;
    }

    //revisado y aprovado por Marcos Escobar
    // SoapCore responde con nombres en PascalCase (Estado, Mensaje); se leen sin distinguir mayúsculas.
    public static ResponseDTO FromXML(string xml)
    {
      XElement cuerpo = Cuerpo(xml);

      string texto = LeerTexto(cuerpo, "estado");
      if (texto.Length == 0)
        throw new InvalidOperationException("Respuesta sin estado");

      EstadosEnum estado = Enum.Parse<EstadosEnum>(texto, true);

      return new ResponseDTO(estado, LeerTexto(cuerpo, "mensaje"));
    }

    //revisado y aprovado por Marcos Escobar
    // ECUD4: el servidor responde «Token inválido o sesión expirada»; el cliente vuelve al inicio de sesión.
    public bool SesionExpirada()
    {
      return estado == EstadosEnum.ERROR && (mensaje ?? "").Contains("Token");
    }

    //revisado y aprovado por Marcos Escobar
    /// <summary>El contenido del soap:Body; un soap:Fault se convierte en SoapFallaException.</summary>
    public static XElement Cuerpo(string xml)
    {
      XElement raiz = XDocument.Parse(xml).Root!;
      XElement body = raiz.Elements().First(e => e.Name.LocalName == "Body");

      XElement? falla = body.Elements().FirstOrDefault(e => e.Name.LocalName == "Fault");
      if (falla is not null)
        throw new SoapFallaException(LeerTexto(falla, "faultstring"));

      return body.Elements().First();
    }

    //revisado y aprovado por Marcos Escobar
    /// <summary>Primer elemento con ese nombre local, en cualquier nivel y sin distinguir mayúsculas.</summary>
    public static string LeerTexto(XElement padre, string nombre)
    {
      XElement? e = padre.Descendants()
        .FirstOrDefault(d => d.Name.LocalName.Equals(nombre, StringComparison.OrdinalIgnoreCase));

      return e?.Value.Trim() ?? "";
    }

    //revisado y aprovado por Marcos Escobar
    public static decimal? LeerDecimal(XElement padre, string nombre)
    {
      return decimal.TryParse(
        LeerTexto(padre, nombre),
        NumberStyles.Number | NumberStyles.AllowExponent,
        CultureInfo.InvariantCulture,
        out decimal valor
      ) ? valor : null;
    }

    //revisado y aprovado por Marcos Escobar
    public static int LeerEntero(XElement padre, string nombre)
    {
      return int.TryParse(LeerTexto(padre, nombre), out int valor) ? valor : 0;
    }
  }

}
