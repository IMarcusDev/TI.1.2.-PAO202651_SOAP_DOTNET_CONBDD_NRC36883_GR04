using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Data;

namespace MOV_EUREKABANK_DOTNET_SOAP.Modelos.Interfaces;

public interface IMovimientoRepositorio
{
    Task<Resultado<ConsultaMovimientos>> ConsultarMovimientosAsync(string token, string cuenta);

    Task<Resultado<Comprobante>> RegistrarAsync(string token, SolicitudOperacion solicitud);
}
