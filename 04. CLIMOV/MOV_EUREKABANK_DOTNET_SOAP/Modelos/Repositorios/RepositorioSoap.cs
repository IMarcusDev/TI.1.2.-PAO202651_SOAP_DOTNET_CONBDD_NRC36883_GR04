using MOV_EUREKABANK_DOTNET_SOAP.Config;
using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Data;
using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Interfaces;
using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Soap;
using System.Globalization;

namespace MOV_EUREKABANK_DOTNET_SOAP.Modelos.Repositorios;

/// <summary>Base de los repositorios SOAP: envía la petición y traduce la respuesta estructurada del servidor.</summary>
public abstract class RepositorioSoap
{
    private readonly string _url;
    private readonly string _contrato;
    private readonly SoapTransporte _transporte;
    private readonly IVerificadorRed _red;

    protected RepositorioSoap(string urlBase, string servicio, string contrato, SoapTransporte transporte, IVerificadorRed red)
    {
        _url = (urlBase.EndsWith('/') ? urlBase : urlBase + "/") + servicio;
        _contrato = contrato;
        _transporte = transporte;
        _red = red;
    }

    protected static SoapConstructorPeticion Peticion(string operacion) => new(SoapConfig.Namespace, operacion);

    protected bool SinRed() => _red.SinRed();

    protected async Task<SoapLectorRespuesta> EnviarAsync(SoapConstructorPeticion peticion)
    {
        string accion = $"{peticion.Namespace}{_contrato}/{peticion.Operacion}";
        return new SoapLectorRespuesta(await _transporte.EnviarAsync(_url, accion, peticion.Construir()));
    }

    /// <summary>Convierte una respuesta con estado ERROR en un resultado, distinguiendo el token inválido.</summary>
    protected static Resultado<T> Fallo<T>(SoapLectorRespuesta respuesta, string porDefecto)
    {
        string mensaje = SoapErrores.MensajeServidor(respuesta.Texto("mensaje"), porDefecto);
        return SoapErrores.EsTokenInvalido(mensaje)
            ? Resultado<T>.Expirada(SoapErrores.SesionExpirada)
            : Resultado<T>.Error(mensaje);
    }

    protected static decimal? Decimal(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return null;
        return decimal.TryParse(texto.Trim().Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal valor)
            ? valor
            : null;
    }

    protected static string TextoOVacio(string? texto) => texto ?? "";
}
