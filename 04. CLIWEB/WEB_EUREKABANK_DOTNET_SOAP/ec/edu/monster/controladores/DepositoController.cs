using ec.edu.monster.filtros;
using ec.edu.monster.modelos.dto;
using ec.edu.monster.modelos.servicios;
using Microsoft.AspNetCore.Mvc;

namespace ec.edu.monster.controladores;

/// <summary>
/// Controller of the deposit screen.
/// GET  /deposito                 -> empty form, or the receipt after a successful deposit
/// POST /deposito paso=revisar    -> validates and shows the confirmation
/// POST /deposito paso=editar     -> back to the form keeping the values
/// POST /deposito paso=confirmar  -> re-validates, calls the SOAP service and redirects (PRG)
/// Employee-only (AuthFilter). The server takes the employee from the token, so the form only
/// has the target account and the amount; a deposit needs no account key.
/// </summary>
[Route("deposito")]
[AuthFilter(SoloEmpleado = true)]
public class DepositoController(MovimientoService movimientoService) : ControladorBase
{
    private const string Vista = "deposito";
    private const string Flash = "comprobanteDeposito";

    [HttpGet("")]
    public IActionResult Index()
    {
        Dictionary<string, string>? recibo = TomarFlash(Flash);
        return recibo is not null
            ? View(Vista, new OperacionVista { Vista = OperacionVista.Comprobante, Recibo = recibo })
            : View(Vista, new OperacionVista());
    }

    [HttpPost("")]
    public async Task<IActionResult> Enviar(string? paso, string? cuenta, string? importe)
    {
        if (paso == "editar")
            return View(Vista, new OperacionVista { Cuenta = cuenta, Importe = importe });

        // Always validated, also on confirm: hidden fields can be tampered with
        MovimientoForm form = MovimientoForm.Deposito(cuenta, importe);
        if (!form.EsValido)
            return View(Vista, new OperacionVista { Cuenta = cuenta, Importe = importe, Errores = form.Errores });

        if (paso != "confirmar")
        {
            return View(Vista, new OperacionVista
            {
                Vista = OperacionVista.Confirmacion,
                Cuenta = form.Cuenta,
                Importe = form.ImporteTexto,
                ImporteFormateado = form.ImporteFormateado,
            });
        }

        MovimientoResultado resultado = await movimientoService.RegistrarDepositoAsync(Token, form.Cuenta, form.Importe!.Value);
        if (resultado.SesionExpirada)
            return SesionExpirada();
        if (!resultado.Ok)
            return View(Vista, new OperacionVista { Cuenta = cuenta, Importe = importe, Error = resultado.Mensaje });

        Dictionary<string, string> recibo = Recibo("Depósito", resultado, form.ImporteFormateado);
        recibo["cuenta"] = form.Cuenta;
        GuardarFlash(Flash, recibo);

        // Post/Redirect/Get: a browser refresh never repeats the deposit
        return Redirect("~/deposito");
    }
}
