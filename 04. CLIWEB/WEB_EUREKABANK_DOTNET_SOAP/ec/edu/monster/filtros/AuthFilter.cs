using ec.edu.monster.controladores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ec.edu.monster.filtros;

/// <summary>
/// Access control for every protected screen, applied BEFORE the action runs.
/// 1. No logged-in user (no session or blank token) -> /login
/// 2. Employee-only screen (SoloEmpleado, e.g. Depósito) and the user is not "Empleado" -> /menu
/// A Cliente reaches Retiro and Transferencia: those controllers always use the account stored
/// in session at login, never one taken from the form.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class AuthFilter : ActionFilterAttribute
{
    public bool SoloEmpleado { get; set; }

    public override void OnActionExecuting(ActionExecutingContext contexto)
    {
        ISession sesion = contexto.HttpContext.Session;

        if (SesionUsuario.TokenDe(sesion) is null)
        {
            contexto.Result = new RedirectResult("~/login");
            return;
        }

        if (SoloEmpleado && !SesionUsuario.EsEmpleado(sesion))
        {
            contexto.Result = new RedirectResult("~/menu");
            return;
        }

        // no-store: after logout, the Back button cannot show a protected page from cache
        contexto.HttpContext.Response.Headers.CacheControl = "no-store";
    }
}
