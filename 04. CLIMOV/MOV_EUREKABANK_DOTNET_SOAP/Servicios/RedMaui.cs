using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Interfaces;

namespace MOV_EUREKABANK_DOTNET_SOAP.Servicios;

public class RedMaui : IVerificadorRed
{
    public bool SinRed()
    {
        try
        {
            return Connectivity.Current.NetworkAccess == NetworkAccess.None;
        }
        catch
        {
            return false;
        }
    }
}
