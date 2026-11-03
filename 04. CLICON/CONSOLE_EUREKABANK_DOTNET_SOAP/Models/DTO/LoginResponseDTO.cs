using CONSOLE_EUREKABANK_DOTNET_SOAP.Models.Enums;
using System.Xml.Linq;

namespace CONSOLE_EUREKABANK_DOTNET_SOAP.Models.DTO
{
  internal class LoginResponseDTO : ResponseDTO
  {
    public string token;
    //revisado y aprovado por Marcos Escobar
    public string perfil = "";
    public string nombre = "";

    public LoginResponseDTO(
      EstadosEnum estado,
      string mensaje,
      string token
    ) : base(estado, mensaje)
    {
      this.token = token;
    }

    public string GetToken()
    {
      return token;
    }

    public void SetToken(string token)
    {
      this.token = token;
    }

    //revisado y aprovado por Marcos Escobar
    public static new LoginResponseDTO FromXML(string xml)
    {
      ResponseDTO response = ResponseDTO.FromXML(xml);
      XElement cuerpo = Cuerpo(xml);

      return new LoginResponseDTO(
        response.GetEstado(),
        response.GetMensaje(),
        LeerTexto(cuerpo, "token")
      )
      {
        perfil = LeerTexto(cuerpo, "perfil"),
        nombre = LeerTexto(cuerpo, "nombre")
      };
    }
  }
}
