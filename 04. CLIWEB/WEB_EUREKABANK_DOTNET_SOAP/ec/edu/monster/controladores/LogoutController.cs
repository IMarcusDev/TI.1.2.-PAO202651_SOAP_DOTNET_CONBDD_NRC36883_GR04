using ec.edu.monster.modelos.servicios;
using Microsoft.AspNetCore.Mvc;

namespace ec.edu.monster.controladores;

/// <summary>
/// Controller of the logout action.
/// POST /logout -> ends the token on the server and the local session, then goes back to /login.
/// Only POST: a link or prefetch (GET) must not log the user out.
/// </summary>
[Route("logout")]
public class LogoutController(LoginService loginService) : Controller
{
    [HttpPost("")]
    public async Task<IActionResult> Salir()
    {
        string? token = SesionUsuario.TokenDe(HttpContext.Session);
        if (token is not null)
        {
            // A failure is ignored: the local session ends anyway
            await loginService.LogoutAsync(token);
        }
        HttpContext.Session.Clear();
        return Redirect("~/login");
    }
}
