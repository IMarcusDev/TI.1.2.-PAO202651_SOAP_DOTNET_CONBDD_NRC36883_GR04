namespace MOV_EUREKABANK_DOTNET_SOAP.Config;

public static class SoapConfig
{
    /// <summary>
    /// Servidor SOAP de EurekaBank (WS_EUREKABANK_DOTNET_SOAP, perfil http de launchSettings).
    /// 10.0.2.2 es el equipo anfitrión visto desde el emulador; en un celular físico use la IP del servidor.
    /// </summary>
    public const string UrlBase = "http://10.0.2.2:5171/";

    /// <summary>Namespace que publica SoapCore para un [ServiceContract] sin Namespace.</summary>
    public const string Namespace = "http://tempuri.org/";

    public const string ServicioLogin = "LoginWebService.svc";
    public const string ServicioMovimiento = "MovimientoWebService.svc";

    /// <summary>Nombre de las interfaces [ServiceContract]; forman la SOAPAction (Namespace + Contrato + "/" + operación).</summary>
    public const string ContratoLogin = "ILoginWebService";
    public const string ContratoMovimiento = "IMovimientoWebService";

    /// <summary>RNF-007: el cliente deja de esperar a los 15 segundos.</summary>
    public const int TiempoEsperaSegundos = 15;
    public static readonly TimeSpan TiempoEspera = TimeSpan.FromSeconds(TiempoEsperaSegundos);
}
