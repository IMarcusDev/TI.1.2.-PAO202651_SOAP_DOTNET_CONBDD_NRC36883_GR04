using ec.edu.monster.filtros;
using Microsoft.AspNetCore.Mvc;

namespace ec.edu.monster.controladores;

/// <summary>
/// Controller of the main menu (home screen after login).
/// GET /menu -> shows the menu; AuthFilter sends a visitor without session to /login.
/// </summary>
[Route("menu")]
[AuthFilter]
public class MenuController : Controller
{
    [HttpGet("")]
    public IActionResult Index() => View("menu");
}
