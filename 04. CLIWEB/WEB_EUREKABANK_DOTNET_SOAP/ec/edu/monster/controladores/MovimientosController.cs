using ec.edu.monster.filtros;
using ec.edu.monster.modelos.dto;
using ec.edu.monster.modelos.servicios;
using Microsoft.AspNetCore.Mvc;

namespace ec.edu.monster.controladores;

/// <summary>
/// Controller of the "Consultar movimientos" screen. Read-only, so it only answers GET.
/// Empleado: GET /movimientos shows the search form; GET /movimientos?cuenta=X queries X.
/// Cliente: always queries the account stored in session at login; the parameter is ignored.
/// Server errors are shown as the server sends them.
/// </summary>
[Route("movimientos")]
[AuthFilter]
public class MovimientosController(MovimientoService movimientoService) : ControladorBase
{
    public const string CuentaVacia = "Ingrese el número de cuenta";

    private const string Vista = "movimientos";

    [HttpGet("")]
    public async Task<IActionResult> Index(string? cuenta)
    {
        // A Cliente can only see their own account: the parameter is ignored
        string? cuentaCliente = CuentaCliente;
        bool esCliente = cuentaCliente is not null;
        string? consultada = esCliente ? cuentaCliente : cuenta;

        // First visit of an employee: only the search form
        if (consultada is null)
            return View(Vista, new MovimientosVista());

        consultada = consultada.Trim();
        if (consultada.Length == 0)
            return View(Vista, new MovimientosVista { Cuenta = consultada, ErrorCuenta = CuentaVacia });

        HistorialCuenta historial = await movimientoService.ConsultarMovimientosAsync(Token, consultada);
        if (historial.SesionExpirada)
            return SesionExpirada();

        return View(Vista, new MovimientosVista
        {
            Cuenta = consultada,
            EsCliente = esCliente,
            Error = historial.Ok ? null : historial.Mensaje,
            Historial = historial.Ok ? historial : null,
        });
    }
}
