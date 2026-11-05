using ec.edu.monster.modelo.dto;

namespace ec.edu.monster.modelo;

/// <summary>
/// Login state of the desktop client: who is logged in and the token the movement services need.
/// Closed when the user logs out.
/// </summary>
public class Sesion
{
    /// <summary>Must match the user type name returned by the server.</summary>
    public const string RolEmpleado = "Empleado";
    public const string RolCliente = "Cliente";

    public string Usuario { get; private set; } = "";

    public string Nombre { get; private set; } = "";

    public string TipoUsuario { get; private set; } = "";

    public string Token { get; private set; } = "";

    public void Iniciar(string usuario, LoginResultado datos)
    {
        Usuario = usuario;
        Nombre = datos.Nombre;
        TipoUsuario = datos.TipoUsuario;
        Token = datos.Token;
    }

    public void Cerrar()
    {
        Usuario = "";
        Nombre = "";
        TipoUsuario = "";
        Token = "";
    }

    public bool EstaActiva => !string.IsNullOrWhiteSpace(Token);

    /// <summary>True for a bank teller: works on any account and is the only one who deposits.</summary>
    public bool EsEmpleado => TipoUsuario == RolEmpleado;

    /// <summary>True for an account holder: works only on the account used to log in.</summary>
    public bool EsCliente => TipoUsuario == RolCliente;

    /// <summary>Account a Cliente logged in with (their user name), or "" for any other role.</summary>
    public string Cuenta => EsCliente ? Usuario : "";

    public string NombreVisible => string.IsNullOrWhiteSpace(Nombre) ? Usuario : Nombre;
}
