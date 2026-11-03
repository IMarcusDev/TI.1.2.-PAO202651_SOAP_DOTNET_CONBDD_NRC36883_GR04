using CONSOLE_EUREKABANK_DOTNET_SOAP.Models.DTO;
using CONSOLE_EUREKABANK_DOTNET_SOAP.Models.Enums;
using CONSOLE_EUREKABANK_DOTNET_SOAP.Models.HTTP;
using CONSOLE_EUREKABANK_DOTNET_SOAP.Models.Data;

namespace CONSOLE_EUREKABANK_DOTNET_SOAP.Controllers
{
  internal class MovimientoController
  {
    //revisado y aprovado por Marcos Escobar
    // ECUD2: datos de la cuenta y lista de sus movimientos.
    public async Task<ConsultaResponseDTO> ConsultarAsync(string cuenta)
    {
      SOAPClient client = new SOAPClient();

      try
      {
        string xml = await client.PostAsync(
            Endpoints.Movimiento,
            Endpoints.ContratoMovimiento,
            Endpoints.Consultar,
            ("token", ConfigHTTP.Token),
            ("cuenta", cuenta)
        );

        return ConsultaResponseDTO.FromXML(xml);
      }
      catch (Exception e)
      {
        return new ConsultaResponseDTO(EstadosEnum.ERROR, ErroresCliente.Mensaje(e));
      }
    }

    //revisado y aprovado por Marcos Escobar
    // ECUD3: el depósito lo registra el empleado de la sesión; no lleva clave.
    public async Task<MovimientoResponseDTO> DepositoAsync(
        string cuenta,
        decimal importe)
    {
      return await RegistrarAsync(
        Endpoints.Deposito,
        ("token", ConfigHTTP.Token),
        ("cuenta", cuenta),
        ("importe", SOAPClient.Importe(importe))
      );
    }

    //revisado y aprovado por Marcos Escobar
    // ECUD3: el empleado envía la clave de la cuenta; el cliente opera su cuenta y la clave va vacía.
    public async Task<MovimientoResponseDTO> RetiroAsync(
        string cuenta,
        decimal importe,
        string clave)
    {
      return await RegistrarAsync(
        Endpoints.Retiro,
        ("token", ConfigHTTP.Token),
        ("cuenta", cuenta),
        ("importe", SOAPClient.Importe(importe)),
        ("clave", clave)
      );
    }

    //revisado y aprovado por Marcos Escobar
    public async Task<MovimientoResponseDTO> TransferenciaAsync(
        string cuentaOrigen,
        string cuentaDestino,
        decimal importe,
        string clave)
    {
      return await RegistrarAsync(
        Endpoints.Transferencia,
        ("token", ConfigHTTP.Token),
        ("cuentaOrigen", cuentaOrigen),
        ("cuentaDestino", cuentaDestino),
        ("importe", SOAPClient.Importe(importe)),
        ("clave", clave)
      );
    }

    //revisado y aprovado por Marcos Escobar
    private async Task<MovimientoResponseDTO> RegistrarAsync(
        string operacion,
        params (string Nombre, string? Valor)[] parametros)
    {
      SOAPClient client = new SOAPClient();

      try
      {
        string xml = await client.PostAsync(
            Endpoints.Movimiento,
            Endpoints.ContratoMovimiento,
            operacion,
            parametros
        );

        return MovimientoResponseDTO.FromXML(xml);
      }
      catch (Exception e)
      {
        return new MovimientoResponseDTO(
            EstadosEnum.ERROR,
            ErroresCliente.Mensaje(e),
            0
        );
      }
    }
  }
}
