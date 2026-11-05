namespace ec.edu.monster.controladores;

/// <summary>Keys of the session values written at login and helpers to read them.</summary>
public static class SesionUsuario
{
    public const string Token = "token";
    public const string Usuario = "usuario";
    public const string Nombre = "nombre";
    public const string TipoUsuario = "tipoUsuario";
    /// <summary>Only for a Cliente: the account typed at login (the server does not return it).</summary>
    public const string Cuenta = "cuenta";

    /// <summary>Must match the user type names returned by the server.</summary>
    public const string RolEmpleado = "Empleado";
    public const string RolCliente = "Cliente";

    /// <summary>Session token, or null when there is no logged-in user.</summary>
    public static string? TokenDe(ISession sesion) => NoVacio(sesion.GetString(Token));

    /// <summary>Account stored at login for a Cliente, or null for an employee.</summary>
    public static string? CuentaCliente(ISession sesion) => NoVacio(sesion.GetString(Cuenta));

    public static bool EsEmpleado(ISession sesion) => sesion.GetString(TipoUsuario) == RolEmpleado;

    private static string? NoVacio(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor;
}
