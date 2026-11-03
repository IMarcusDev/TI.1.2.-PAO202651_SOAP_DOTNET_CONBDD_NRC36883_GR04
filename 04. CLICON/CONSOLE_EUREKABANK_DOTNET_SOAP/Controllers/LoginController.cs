using CONSOLE_EUREKABANK_DOTNET_SOAP.Models.DTO;
using CONSOLE_EUREKABANK_DOTNET_SOAP.Models.Enums;
using CONSOLE_EUREKABANK_DOTNET_SOAP.Models.HTTP;
using CONSOLE_EUREKABANK_DOTNET_SOAP.Models.Data;

namespace CONSOLE_EUREKABANK_DOTNET_SOAP.Controllers
{
  internal class LoginController
  {
    //revisado y aprovado por Marcos Escobar
    // ECUD1: el empleado ingresa con su usuario y clave.
    public async Task<LoginResponseDTO> LoginEmpleadoAsync(
        string usuario,
        string clave)
    {
      return await IniciarSesionAsync(Endpoints.LoginEmpleado, null, ("usuario", usuario), ("clave", clave));
    }

    //revisado y aprovado por Marcos Escobar
    // ECUD1: el cliente ingresa con el número de su cuenta y la clave de esa cuenta.
    public async Task<LoginResponseDTO> LoginClienteAsync(
        string cuenta,
        string clave)
    {
      return await IniciarSesionAsync(Endpoints.LoginCliente, cuenta, ("cuenta", cuenta), ("clave", clave));
    }

    //revisado y aprovado por Marcos Escobar
    // Cierra la sesión en el servidor; los datos locales se borran aunque el servidor no responda.
    public async Task<ResponseDTO> LogoutAsync()
    {
      SOAPClient client = new SOAPClient();

      try
      {
        string xml = await client.PostAsync(
            Endpoints.Login,
            Endpoints.ContratoLogin,
            Endpoints.Logout,
            ("token", ConfigHTTP.Token)
        );

        return ResponseDTO.FromXML(xml);
      }
      catch (Exception e)
      {
        return new ResponseDTO(EstadosEnum.ERROR, ErroresCliente.Mensaje(e));
      }
      finally
      {
        ConfigHTTP.Limpiar();
      }
    }

    //revisado y aprovado por Marcos Escobar
    private async Task<LoginResponseDTO> IniciarSesionAsync(
        string operacion,
        string? cuenta,
        params (string Nombre, string? Valor)[] parametros)
    {
      SOAPClient client = new SOAPClient();

      try
      {
        string xml = await client.PostAsync(
            Endpoints.Login,
            Endpoints.ContratoLogin,
            operacion,
            parametros
        );

        LoginResponseDTO dto = LoginResponseDTO.FromXML(xml);

        if (dto.GetEstado() == EstadosEnum.OK)
        {
          ConfigHTTP.Token = dto.GetToken();
          ConfigHTTP.Perfil = dto.perfil;
          ConfigHTTP.Nombre = dto.nombre;
          ConfigHTTP.Cuenta = cuenta ?? "";
        }

        return dto;
      }
      catch (Exception e)
      {
        return new LoginResponseDTO(
            EstadosEnum.ERROR,
            ErroresCliente.Mensaje(e),
            ""
        );
      }
    }
  }

}
