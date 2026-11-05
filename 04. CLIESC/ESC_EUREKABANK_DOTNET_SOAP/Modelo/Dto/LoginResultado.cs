namespace ec.edu.monster.modelo.dto;

/// <summary>Answer of loginEmpleado / loginCliente: on success, the token and the user data.</summary>
public record LoginResultado(bool Ok, string Mensaje, string Token = "", string Nombre = "", string TipoUsuario = "")
{
    public static LoginResultado Error(string mensaje) => new(false, mensaje);
}
