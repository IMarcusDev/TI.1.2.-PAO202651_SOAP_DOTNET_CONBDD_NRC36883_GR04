using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Interfaces;
using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Soap;

namespace MOV_EUREKABANK_DOTNET_SOAP.Pruebas;

/// <summary>Transporte que no abre conexiones: registra cada petición y devuelve la respuesta indicada.</summary>
public class TransporteFalso(Func<string, string> responder) : SoapTransporte
{
    public record Peticion(string Url, string Accion, string Sobre);

    public List<Peticion> Peticiones { get; } = [];

    public Peticion Ultima => Peticiones[^1];

    public static TransporteFalso Responde(string xml) => new(_ => xml);

    public static TransporteFalso Lanza(Exception error) => new(_ => throw error);

    public override Task<string> EnviarAsync(string url, string accion, string sobre)
    {
        Peticiones.Add(new Peticion(url, accion, sobre));
        return Task.FromResult(responder(sobre));
    }

    /// <summary>Respuesta como la publica SoapCore: elementos en PascalCase dentro de {operación}Result.</summary>
    public static string Respuesta(string operacion, string contenido)
        => "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\"><s:Body>"
         + $"<{operacion}Response xmlns=\"http://tempuri.org/\"><{operacion}Result>{contenido}</{operacion}Result>"
         + $"</{operacion}Response></s:Body></s:Envelope>";

    public static string Falla(string faultstring)
        => "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\"><s:Body><s:Fault>"
         + $"<faultcode>s:Server</faultcode><faultstring>{faultstring}</faultstring>"
         + "</s:Fault></s:Body></s:Envelope>";
}

public class RedFalsa(bool sinRed) : IVerificadorRed
{
    public bool SinRed() => sinRed;
}
