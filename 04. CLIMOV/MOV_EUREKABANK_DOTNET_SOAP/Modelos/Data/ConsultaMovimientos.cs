namespace MOV_EUREKABANK_DOTNET_SOAP.Modelos.Data;

public sealed record ConsultaMovimientos(Cuenta Cuenta, IReadOnlyList<Movimiento> Movimientos);
