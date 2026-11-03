namespace WS_CON_UNI_DOTNET_SOAP.Models.Data
{
  public class Movimiento
  {
    public string Id { get; set; } = "";
    public string Tipo { get; set; } = "";
    public string CuentaOrigen { get; set; } = "";
    public string CuentaDestino { get; set; } = "";
    public double Valor { get; set; }
  }
}
