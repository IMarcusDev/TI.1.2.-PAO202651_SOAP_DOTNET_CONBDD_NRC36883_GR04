using System.ServiceModel;
using WS_EUREKABANK_DOTNET_SOAP.Models.DTO;

namespace WS_EUREKABANK_DOTNET_SOAP.WS.Interfaces
{
  [ServiceContract]
  public interface IMovimientoWebService
  {
    [OperationContract(Name = "deposito")]
    MovimientoResponseDTO Deposito(
      string token,
      string cuentaOrigen,
      string cuentaDestino,
      double valor
    );

    [OperationContract(Name = "retiro")]
    MovimientoResponseDTO Retiro(
      string token,
      string cuentaOrigen,
      string cuentaDestino,
      double valor
    );

    [OperationContract(Name = "transferencia")]
    MovimientoResponseDTO Transferencia(
      string token,
      string cuentaOrigen,
      string cuentaDestino,
      double valor
    );

    //revisado y aprovado por Marcos Escobar
    [OperationContract(Name = "consultarMovimientos")]
    ConsultaMovimientosResponseDTO ConsultarMovimientos(string token, string cuenta);

    //revisado y aprovado por Marcos Escobar
    [OperationContract(Name = "registrarDeposito")]
    MovimientoResponseDTO RegistrarDeposito(string token, string cuenta, decimal importe);

    //revisado y aprovado por Marcos Escobar
    [OperationContract(Name = "registrarRetiro")]
    MovimientoResponseDTO RegistrarRetiro(string token, string cuenta, decimal importe, string clave);

    //revisado y aprovado por Marcos Escobar
    [OperationContract(Name = "registrarTransferencia")]
    MovimientoResponseDTO RegistrarTransferencia(
      string token,
      string cuentaOrigen,
      string cuentaDestino,
      decimal importe,
      string clave
    );
  }
}