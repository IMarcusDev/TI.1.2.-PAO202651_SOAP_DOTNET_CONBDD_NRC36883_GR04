using ec.edu.monster.filtros;
using ec.edu.monster.modelos.dto;
using ec.edu.monster.modelos.servicios;
using Microsoft.AspNetCore.Mvc;

namespace ec.edu.monster.controladores;

/// <summary>
/// Controller of the withdrawal screen.
/// GET  /retiro                 -> empty form, or the receipt after a successful withdrawal
/// POST /retiro paso=revisar    -> validates and shows the confirmation
/// POST /retiro paso=editar     -> back to the form keeping the values
/// POST /retiro paso=confirmar  -> re-validates, calls the SOAP service and redirects (PRG)
/// Empleado: any account; the account key is asked on the confirmation step.
/// Cliente: always the account stored in session at login, no key (sent as "").
/// The key is only read on confirm: it is never put in the view or the session.
/// </summary>
[Route("retiro")]
[AuthFilter]
public class RetiroController(MovimientoService movimientoService) : ControladorBase
{
    public const string ClaveVacia = "Ingrese la clave de la cuenta";

    private const string Vista = "retiro";
    private const string Flash = "comprobanteRetiro";

    [HttpGet("")]
    public IActionResult Index()
    {
        Dictionary<string, string>? recibo = TomarFlash(Flash);
        if (recibo is not null)
            return View(Vista, new OperacionVista { Vista = OperacionVista.Comprobante, Recibo = recibo });

        string? cuentaCliente = CuentaCliente;
        return View(Vista, new OperacionVista { Cuenta = cuentaCliente, EsCliente = cuentaCliente is not null });
    }

    [HttpPost("")]
    public async Task<IActionResult> Enviar(string? paso, string? cuenta, string? importe, string? clave)
    {
        // A Cliente can only use their own account: the form value is ignored
        string? cuentaCliente = CuentaCliente;
        bool esCliente = cuentaCliente is not null;
        if (esCliente)
            cuenta = cuentaCliente;

        if (paso == "editar")
            return Formulario(cuenta, importe, esCliente);

        // Always validated, also on confirm: hidden fields can be tampered with
        MovimientoForm form = MovimientoForm.Retiro(cuenta, importe);
        if (!form.EsValido)
            return Formulario(cuenta, importe, esCliente, errores: form.Errores);

        if (paso != "confirmar")
            return Confirmacion(form, esCliente);

        string claveEnviada = "";
        if (!esCliente)
        {
            // Sent as typed (no trim): the server compares it with the stored key
            if (string.IsNullOrWhiteSpace(clave))
                return Confirmacion(form, esCliente, new Dictionary<string, string> { ["clave"] = ClaveVacia });
            claveEnviada = clave;
        }

        MovimientoResultado resultado = await movimientoService.RegistrarRetiroAsync(
            Token, form.Cuenta, form.Importe!.Value, claveEnviada);
        if (resultado.SesionExpirada)
            return SesionExpirada();
        if (!resultado.Ok)
            return Formulario(cuenta, importe, esCliente, error: resultado.Mensaje);

        // Never the key
        Dictionary<string, string> recibo = Recibo("Retiro", resultado, form.ImporteFormateado);
        recibo["cuenta"] = form.Cuenta;
        GuardarFlash(Flash, recibo);

        // Post/Redirect/Get: a browser refresh never repeats the withdrawal
        return Redirect("~/retiro");
    }

    // Form state keeping what the user typed
    private ViewResult Formulario(string? cuenta, string? importe, bool esCliente,
        IReadOnlyDictionary<string, string>? errores = null, string? error = null)
        => View(Vista, new OperacionVista
        {
            Cuenta = cuenta,
            Importe = importe,
            EsCliente = esCliente,
            Errores = errores ?? new Dictionary<string, string>(),
            Error = error,
        });

    // Confirmation state with the validated values (the key input is always rendered empty)
    private ViewResult Confirmacion(MovimientoForm form, bool esCliente, IReadOnlyDictionary<string, string>? errores = null)
        => View(Vista, new OperacionVista
        {
            Vista = OperacionVista.Confirmacion,
            Cuenta = form.Cuenta,
            Importe = form.ImporteTexto,
            ImporteFormateado = form.ImporteFormateado,
            EsCliente = esCliente,
            Errores = errores ?? new Dictionary<string, string>(),
        });
}
