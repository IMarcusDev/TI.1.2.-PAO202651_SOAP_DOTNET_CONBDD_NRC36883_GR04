namespace MOV_EUREKABANK_DOTNET_SOAP.Modelos.Enums;

public enum TipoOperacion
{
    Deposito,
    Retiro,
    Transferencia,
}

public static class TipoOperacionExtensions
{
    public static string OperacionSoap(this TipoOperacion tipo) => tipo switch
    {
        TipoOperacion.Deposito => "registrarDeposito",
        TipoOperacion.Retiro => "registrarRetiro",
        _ => "registrarTransferencia",
    };
}
