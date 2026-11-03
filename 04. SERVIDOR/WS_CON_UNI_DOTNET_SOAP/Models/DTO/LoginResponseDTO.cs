namespace WS_CON_UNI_DOTNET_SOAP.Models.DTO
{
  public class LoginResponseDTO : ResponseDTO
  {
    public string Token { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string EstadoUsuario { get; set; } = "";
    public string TipoUsuario { get; set; } = "";
  }
}
