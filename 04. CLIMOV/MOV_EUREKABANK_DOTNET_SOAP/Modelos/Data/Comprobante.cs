namespace MOV_EUREKABANK_DOTNET_SOAP.Modelos.Data;

/// <param name="NuevoSaldo">Nuevo saldo de la cuenta, o de la cuenta de origen en una transferencia.</param>
public sealed record Comprobante(SolicitudOperacion Solicitud, string NumeroMovimiento, decimal? NuevoSaldo, DateTime Fecha);
