using System.ServiceModel;
using WS_EUREKABANK_DOTNET_SOAP.Models.DTO;

namespace WS_EUREKABANK_DOTNET_SOAP.WS.Interfaces
{
  [ServiceContract]
  public interface ILoginWebService
  {
    [OperationContract]
    LoginResponseDTO Login(string username, string password);

    //revisado y aprovado por Marcos Escobar
    [OperationContract(Name = "loginEmpleado")]
    LoginResponseDTO LoginEmpleado(string usuario, string clave);

    //revisado y aprovado por Marcos Escobar
    [OperationContract(Name = "loginCliente")]
    LoginResponseDTO LoginCliente(string cuenta, string clave);

    //revisado y aprovado por Marcos Escobar
    [OperationContract(Name = "logout")]
    LoginResponseDTO Logout(string token);
  }
}