using ec.edu.monster.modelo;
using ec.edu.monster.modelo.dto;
using ec.edu.monster.modelo.servicios;

namespace ec.edu.monster.controlador;

/// <summary>
/// Handles the login screen: validates the input, asks the service to log in and starts the
/// session. It never touches a window, so it is testable without WPF.
/// </summary>
public sealed class LoginControlador(LoginServicio servicio, Sesion sesion)
{
    public const string CredencialesVaciasEmpleado = "Ingrese usuario y contraseña";
    public const string CredencialesVaciasCliente = "Ingrese número de cuenta y clave";

    /// <returns>null when the session started; the message to show otherwise.</returns>
    public async Task<string?> IngresarAsync(string usuario, string clave, bool esCliente)
    {
        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(clave))
            return esCliente ? CredencialesVaciasCliente : CredencialesVaciasEmpleado;

        string identificador = usuario.Trim();
        LoginResultado datos = esCliente
            ? await servicio.LoginClienteAsync(identificador, clave)
            : await servicio.LoginEmpleadoAsync(identificador, clave);

        if (!datos.Ok)
            return datos.Mensaje;

        sesion.Iniciar(identificador, datos);
        return null;
    }

    /// <summary>Ends the session locally and, best effort, on the server.</summary>
    public async Task CerrarSesionAsync()
    {
        string token = sesion.Token;
        sesion.Cerrar();
        if (!string.IsNullOrWhiteSpace(token))
            await servicio.LogoutAsync(token);
    }
}
