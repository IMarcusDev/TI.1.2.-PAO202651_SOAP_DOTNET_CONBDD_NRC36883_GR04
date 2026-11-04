namespace WS_EUREKABANK_DOTNET_SOAP.Models.DTO
{
  public class LoginResponseDTO : ResponseDTO
  {
    public string Token { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string EstadoUsuario { get; set; } = "";
    public string TipoUsuario { get; set; } = "";
    //revisado y aprovado por Marcos Escobar
    public string Perfil { get; set; } = "";
  }
}
