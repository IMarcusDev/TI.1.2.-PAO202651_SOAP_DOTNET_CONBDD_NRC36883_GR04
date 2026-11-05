namespace ec.edu.monster.config;

/// <summary>
/// Fixed parts of the SOAP contract published by WS_EUREKABANK_DOTNET_SOAP (SoapCore).
/// They are not configuration: the user only changes Host and Puerto.
/// </summary>
public static class RutasSoap
{
    /// <summary>Namespace SoapCore publishes for a [ServiceContract] without Namespace.</summary>
    public const string Namespace = "http://tempuri.org/";

    public const string ServicioLogin = "LoginWebService.svc";
    public const string ServicioMovimiento = "MovimientoWebService.svc";

    /// <summary>[ServiceContract] interface names; SOAPAction = Namespace + Contrato + "/" + operation.</summary>
    public const string ContratoLogin = "ILoginWebService";
    public const string ContratoMovimiento = "IMovimientoWebService";

    /// <summary>The client stops waiting after 15 seconds.</summary>
    public static readonly TimeSpan TiempoEspera = TimeSpan.FromSeconds(15);
}
