using System.ServiceModel;
using WS_CON_UNI_DOTNET_SOAP.Models.DTO;

namespace WS_CON_UNI_DOTNET_SOAP.WS.Interfaces
{
  [ServiceContract]
  public interface ILoginWebService
  {
    [OperationContract]
    LoginResponseDTO Login(string username, string password);
  }
}