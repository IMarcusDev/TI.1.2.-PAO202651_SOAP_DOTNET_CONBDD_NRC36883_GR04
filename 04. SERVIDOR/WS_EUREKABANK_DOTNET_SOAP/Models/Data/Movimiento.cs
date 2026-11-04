namespace WS_EUREKABANK_DOTNET_SOAP.Models.Data
{
  public class Movimiento
  {
    public string Id { get; set; } = "";
    public string Tipo { get; set; } = "";
    public string CuentaOrigen { get; set; } = "";
    public string CuentaDestino { get; set; } = "";
    public double Valor { get; set; }
    //revisado y aprovado por Marcos Escobar
    public DateTime? Fecha { get; set; }
    //revisado y aprovado por Marcos Escobar
    public double NuevoSaldo { get; set; }
  }
}
