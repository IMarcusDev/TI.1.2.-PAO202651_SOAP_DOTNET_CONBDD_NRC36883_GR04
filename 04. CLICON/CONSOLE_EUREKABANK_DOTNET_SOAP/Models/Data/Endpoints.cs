namespace CONSOLE_EUREKABANK_DOTNET_SOAP.Models.Data
{
  /// <summary>Servicios SOAP del servidor .NET (SoapCore) y operaciones del ERS (RNF-001).</summary>
  //revisado y aprovado por Marcos Escobar
  public class Endpoints
  {
    // Namespace que publica SoapCore para un [ServiceContract] sin Namespace.
    public const string Namespace = "http://tempuri.org/";

    public static string Login { get; set; } = "/LoginWebService.svc";
    public static string Movimiento { get; set; } = "/MovimientoWebService.svc";

    public const string ContratoLogin = "ILoginWebService";
    public const string ContratoMovimiento = "IMovimientoWebService";

    public const string LoginEmpleado = "loginEmpleado";
    public const string LoginCliente = "loginCliente";
    public const string Logout = "logout";
    public const string Consultar = "consultarMovimientos";
    public const string Deposito = "registrarDeposito";
    public const string Retiro = "registrarRetiro";
    public const string Transferencia = "registrarTransferencia";
  }
}
