namespace MOV_EUREKABANK_DOTNET_SOAP.Modelos.Data;

/// <summary>Respuesta estructurada (RQF-006) ya interpretada por el modelo: estado, mensaje y datos.</summary>
public sealed class Resultado<T>
{
    private Resultado(bool exito, string mensaje, T? datos, bool sesionExpirada)
    {
        Exito = exito;
        Mensaje = mensaje;
        Datos = datos;
        SesionExpirada = sesionExpirada;
    }

    public bool Exito { get; }

    public string Mensaje { get; }

    public T? Datos { get; }

    public bool SesionExpirada { get; }

    public static Resultado<T> Ok(T datos, string mensaje) => new(true, mensaje, datos, false);

    public static Resultado<T> Error(string mensaje) => new(false, mensaje, default, false);

    public static Resultado<T> Expirada(string mensaje) => new(false, mensaje, default, true);
}
