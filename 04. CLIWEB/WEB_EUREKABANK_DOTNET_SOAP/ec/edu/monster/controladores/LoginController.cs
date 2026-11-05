using ec.edu.monster.modelos.dto;
using ec.edu.monster.modelos.servicios;
using Microsoft.AspNetCore.Mvc;

namespace ec.edu.monster.controladores;

/// <summary>
/// Controller of the login screen.
/// GET  /login            -> shows the form (Empleado selected)
/// GET  /login?expirada=1 -> same, with the "session expired" notice
/// POST /login            -> validates the credentials against the SOAP server
/// tipo=empleado -> loginEmpleado(usuario, clave); tipo=cliente -> loginCliente(cuenta, clave).
/// Anything else counts as empleado.
/// </summary>
[Route("login")]
public class LoginController(LoginService loginService) : Controller
{
    public const string TipoEmpleado = "empleado";
    public const string TipoCliente = "cliente";
    public const string AvisoExpirada = "Su sesión expiró o no es válida. Inicie sesión nuevamente.";

    private const string Vista = "login";

    [HttpGet("")]
    public IActionResult Index(string? expirada)
        => View(Vista, new LoginVista { Aviso = expirada == "1" ? AvisoExpirada : null });

    [HttpPost("")]
    public async Task<IActionResult> Ingresar(string? tipo, string? username, string? password)
    {
        bool esCliente = string.Equals(tipo, TipoCliente, StringComparison.OrdinalIgnoreCase);
        string tipoElegido = esCliente ? TipoCliente : TipoEmpleado;

        // Avoid a SOAP call when the form is incomplete
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            string mensaje = esCliente ? "Ingrese número de cuenta y clave" : "Ingrese usuario y contraseña";
            return View(Vista, new LoginVista { Tipo = tipoElegido, Username = username, Error = mensaje });
        }

        string identificador = username.Trim();
        LoginResultado resultado = esCliente
            ? await loginService.LoginClienteAsync(identificador, password)
            : await loginService.LoginEmpleadoAsync(identificador, password);

        if (!resultado.Ok)
        {
            // Wrong credentials, inactive account or connection problem: the message comes from the model
            return View(Vista, new LoginVista { Tipo = tipoElegido, Username = username, Error = resultado.Mensaje });
        }

        // Nothing from a previous user survives in the session
        ISession sesion = HttpContext.Session;
        sesion.Clear();
        sesion.SetString(SesionUsuario.Token, resultado.Token);
        sesion.SetString(SesionUsuario.Usuario, identificador);
        sesion.SetString(SesionUsuario.Nombre, resultado.Nombre);
        sesion.SetString(SesionUsuario.TipoUsuario, resultado.TipoUsuario);
        if (esCliente)
        {
            // The server ties the token to this account but does not return it:
            // operations of a Cliente always use this value, never one from the form
            sesion.SetString(SesionUsuario.Cuenta, identificador);
        }

        // Redirect (not a view): a browser refresh will not resend the password
        return Redirect("~/menu");
    }
}
