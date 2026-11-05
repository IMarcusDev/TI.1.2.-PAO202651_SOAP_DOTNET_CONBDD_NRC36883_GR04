using ec.edu.monster.config;
using ec.edu.monster.modelo.soap;

namespace ec.edu.monster.modelo.servicios;

/// <summary>Base of the SOAP services: sends the request and turns failures into messages.</summary>
public abstract class ServicioSoap
{
    public const string RespuestaIncompleta = "El servidor envió una respuesta vacía o incompleta.";
    public const string FallaServidor = "El servidor no pudo procesar la solicitud. Intente más tarde.";
    public const string SinRespuestaATiempo = "El servidor no respondió a tiempo. La operación pudo "
        + "haberse registrado: verifique el saldo antes de volver a intentarlo";

    private readonly ITransporte _transporte;
    private readonly string _contrato;

    protected ServicioSoap(ITransporte transporte, Configuracion configuracion, string servicio, string contrato)
    {
        _transporte = transporte;
        _contrato = contrato;
        Url = configuracion.Url(servicio);
    }

    /// <summary>Address of the service, also shown when the server cannot be reached.</summary>
    public string Url { get; }

    protected string SinConexion => "No se pudo conectar con el servidor: " + Url;

    protected static SoapConstructorPeticion Peticion(string operacion) => new(RutasSoap.Namespace, operacion);

    protected async Task<SoapLectorRespuesta> EnviarAsync(SoapConstructorPeticion peticion)
    {
        string accion = $"{peticion.Namespace}{_contrato}/{peticion.Operacion}";
        return new SoapLectorRespuesta(await _transporte.EnviarAsync(Url, accion, peticion.Construir()));
    }

    /// <summary>User message for an exception thrown while calling the server.</summary>
    protected string Mensaje(Exception ex) => ex switch
    {
        SoapFallaException => FallaServidor,
        RespuestaInvalidaException => RespuestaIncompleta,
        _ => SinConexion,
    };

    /// <summary>The server message, or the default when it sent none.</summary>
    protected static string MensajeServidor(string? mensaje, string porDefecto)
        => string.IsNullOrWhiteSpace(mensaje) ? porDefecto : mensaje.Trim();

    /// <summary>The server says "Token inválido o sesión expirada" when the token is no longer valid.</summary>
    public static bool EsTokenInvalido(string? mensaje)
        => mensaje is not null && mensaje.Contains("token", StringComparison.OrdinalIgnoreCase);
}
