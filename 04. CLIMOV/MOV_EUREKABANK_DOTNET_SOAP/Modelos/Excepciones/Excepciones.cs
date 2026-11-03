namespace MOV_EUREKABANK_DOTNET_SOAP.Modelos.Excepciones;

public class HttpEstadoException(int codigo, string url) : IOException($"HTTP {codigo} en {url}")
{
    public int Codigo { get; } = codigo;
}

public class RespuestaInvalidaException(string mensaje, Exception? causa = null) : Exception(mensaje, causa);

public class SoapFallaException(string faultstring) : Exception(faultstring);
