using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Enums;

namespace MOV_EUREKABANK_DOTNET_SOAP.Modelos.Data;

public sealed record ValidacionResultado(bool Valido, Campo Campo, string? Mensaje)
{
    public static readonly ValidacionResultado Valida = new(true, Campo.Ninguno, null);

    public static ValidacionResultado Error(Campo campo, string mensaje) => new(false, campo, mensaje);
}
