namespace WS_EUREKABANK_DOTNET_SOAP.Models.DTO
{
  public class MovimientoResponseDTO : ResponseDTO
  {
    public int IdMovimiento { get; set; }
    //revisado y aprovado por Marcos Escobar
    public int NumeroMovimiento { get; set; }
    //revisado y aprovado por Marcos Escobar
    public decimal? NuevoSaldo { get; set; }
  }
}
