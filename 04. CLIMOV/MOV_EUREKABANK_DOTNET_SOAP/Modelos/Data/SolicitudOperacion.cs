using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Enums;

namespace MOV_EUREKABANK_DOTNET_SOAP.Modelos.Data;

/// <summary>Datos ya validados de un depósito, retiro o transferencia, listos para enviar al servidor.</summary>
/// <param name="Cuenta">Cuenta del depósito o del retiro, o cuenta de origen de la transferencia.</param>
/// <param name="Clave">Clave de la cuenta; vacía cuando opera el propio cliente o en un depósito.</param>
public sealed record SolicitudOperacion(TipoOperacion Tipo, string Cuenta, string? CuentaDestino,
                                        decimal Importe, string Clave);
