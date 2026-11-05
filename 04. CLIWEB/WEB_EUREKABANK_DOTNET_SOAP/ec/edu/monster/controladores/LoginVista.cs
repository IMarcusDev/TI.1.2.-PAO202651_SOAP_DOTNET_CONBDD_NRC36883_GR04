namespace ec.edu.monster.controladores;

/// <summary>Data of the login view.</summary>
public class LoginVista
{
    /// <summary>Selected role: empleado (default) | cliente.</summary>
    public string Tipo { get; init; } = LoginController.TipoEmpleado;

    /// <summary>Value typed by the user; the password is never sent back.</summary>
    public string? Username { get; init; }

    public string? Error { get; init; }

    /// <summary>Notice shown after the server ended the session.</summary>
    public string? Aviso { get; init; }

    public bool EsCliente => Tipo == LoginController.TipoCliente;
}
