using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Excepciones;
using System.Net.Sockets;

namespace MOV_EUREKABANK_DOTNET_SOAP.Modelos.Repositorios;

public static class SoapErrores
{
    public const string SinConexion = "Sin conexión de red. Revise el Wi-Fi e intente nuevamente.";
    public const string SinServidor = "No se pudo conectar con el servidor. Verifique la red e intente nuevamente.";
    public const string RespuestaIncompleta = "El servidor envió una respuesta vacía o incompleta.";
    public const string SesionExpirada = "Su sesión expiró o no es válida. Inicie sesión nuevamente.";

    /// <summary>Excepciones de Java que Android entrega envueltas cuando falla la conexión.</summary>
    private static readonly string[] ErroresDeRedJava =
        ["IOException", "ConnectException", "UnknownHostException", "NoRouteToHostException", "SocketTimeoutException"];

    public static string Mensaje(Exception ex)
    {
        string mensaje = MensajeAmigable(ex);
#if DEBUG
        Exception causa = ex.GetBaseException();
        mensaje += $"\n[Debug] {causa.GetType().Name}: {causa.Message}";
#endif
        return mensaje;
    }

    private static string MensajeAmigable(Exception ex)
    {
        if (ex is SoapFallaException)
            return "El servidor no pudo procesar la solicitud. Intente más tarde.";
        if (ex is RespuestaInvalidaException)
            return RespuestaIncompleta;
        if (ex is UriFormatException)
            return "La dirección del servidor está mal configurada.";
        // ECUD: sin respuesta en 15 s, servidor apagado o fuera de la red.
        if (Causas(ex).Any(EsErrorDeRed))
            return SinServidor;
        return "Ocurrió un error inesperado al contactar al servidor.";
    }

    private static IEnumerable<Exception> Causas(Exception ex)
    {
        for (Exception? actual = ex; actual is not null; actual = actual.InnerException)
            yield return actual;
    }

    private static bool EsErrorDeRed(Exception ex)
        => ex is IOException or HttpRequestException or SocketException or OperationCanceledException or TimeoutException
           || ErroresDeRedJava.Contains(ex.GetType().Name);

    public static string MensajeServidor(string? mensaje, string porDefecto)
        => string.IsNullOrWhiteSpace(mensaje) ? porDefecto : mensaje.Trim();

    /// <summary>ECUD4: "Token inválido o sesión expirada".</summary>
    public static bool EsTokenInvalido(string? mensaje)
    {
        if (mensaje is null)
            return false;
        string texto = mensaje.ToLowerInvariant();
        return texto.Contains("token") || texto.Contains("sesión expirada") || texto.Contains("sesion expirada");
    }
}
