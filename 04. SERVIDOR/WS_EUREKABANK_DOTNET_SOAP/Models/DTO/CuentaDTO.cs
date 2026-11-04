namespace WS_EUREKABANK_DOTNET_SOAP.Models.DTO
{
  /// <summary>Datos de la cuenta que devuelve consultarMovimientos.</summary>
  //revisado y aprovado por Marcos Escobar
  public class CuentaDTO
  {
    // La base de datos no guarda la moneda: EurekaBank trabaja solo en dólares.
    public const string MONEDA = "USD";

    public string NumeroCuenta { get; set; } = "";
    public string Titular { get; set; } = "";
    public string Moneda { get; set; } = MONEDA;
    public decimal Saldo { get; set; }
    public string EstadoCuenta { get; set; } = "";
  }
}
