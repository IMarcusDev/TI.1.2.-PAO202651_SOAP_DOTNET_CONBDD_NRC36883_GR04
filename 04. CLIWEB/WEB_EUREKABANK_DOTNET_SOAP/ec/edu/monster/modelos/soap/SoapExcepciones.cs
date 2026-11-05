namespace ec.edu.monster.modelos.soap;

/// <summary>The server answered with an HTTP status that carries no SOAP envelope.</summary>
public class HttpEstadoException(int codigo, string url) : IOException($"HTTP {codigo} en {url}")
{
    public int Codigo { get; } = codigo;
}

/// <summary>The response is empty or is not XML.</summary>
public class RespuestaInvalidaException(string mensaje, Exception? causa = null) : Exception(mensaje, causa);

/// <summary>The server answered with a SOAP Fault.</summary>
public class SoapFallaException(string faultstring) : Exception(faultstring);
