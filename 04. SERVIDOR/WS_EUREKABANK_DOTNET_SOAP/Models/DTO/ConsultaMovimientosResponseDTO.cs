namespace WS_CON_UNI_DOTNET_SOAP.Models.DTO
{
  /// <summary>Respuesta de consultarMovimientos: los datos de la cuenta y sus movimientos.</summary>
  //revisado y aprovado por Marcos Escobar
  public class ConsultaMovimientosResponseDTO : ResponseDTO
  {
    public CuentaDTO? Cuenta { get; set; }
    public List<MovimientoDTO> Movimientos { get; set; } = new List<MovimientoDTO>();
  }
}
