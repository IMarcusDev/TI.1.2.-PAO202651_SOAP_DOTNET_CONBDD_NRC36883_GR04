using System.ServiceModel;
using WS_CON_UNI_DOTNET_SOAP.Models.DTO;

namespace WS_CON_UNI_DOTNET_SOAP.WS.Interfaces
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
  }
}