namespace MOV_EUREKABANK_DOTNET_SOAP.Modelos.Enums;

public enum Perfil
{
    Empleado,
    Cliente,
}

public static class PerfilExtensions
{
    public static string Etiqueta(this Perfil perfil) => perfil == Perfil.Cliente ? "Cliente" : "Empleado";

    /// <summary>Interpreta el perfil que devuelve el servidor; si no lo reconoce, usa el que se pidió al iniciar sesión.</summary>
    public static Perfil Desde(string? texto, Perfil porDefecto)
    {
        if (texto is null)
            return porDefecto;

        string normalizado = texto.Trim().ToUpperInvariant();
        if (normalizado.Contains("EMPLEADO"))
            return Perfil.Empleado;
        if (normalizado.Contains("CLIENTE"))
            return Perfil.Cliente;
        return porDefecto;
    }
}
