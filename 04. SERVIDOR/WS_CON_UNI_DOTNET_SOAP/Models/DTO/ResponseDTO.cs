using WS_CON_UNI_DOTNET_SOAP.Models.Enums;

namespace WS_CON_UNI_DOTNET_SOAP.Models.DTO
{
  public class ResponseDTO
  {
    public EstadosEnum Estado { get; set; }
    public string Mensaje { get; set; } = "";
  }
}
