using ec.edu.monster.filtros;
using ec.edu.monster.modelos.dto;
using ec.edu.monster.modelos.servicios;
using Microsoft.AspNetCore.Mvc;

namespace ec.edu.monster.controladores;

/// <summary>
/// Controller of the transfer screen.
/// GET  /transferencia                 -> empty form, or the receipt after a successful transfer
/// POST /transferencia paso=revisar    -> validates and shows the confirmation
/// POST /transferencia paso=editar     -> back to the form keeping the values
/// POST /transferencia paso=confirmar  -> re-validates, calls the SOAP service and redirects (PRG)
/// Empleado: any source account; its key is asked on the confirmation step.
/// Cliente: the source is always the account stored in session at login, no key (sent as "").
/// The key is only read on confirm: it is never put in the view or the session.
/// </summary>
[Route("transferencia")]
[AuthFilter]
public class TransferenciaController(MovimientoService movimientoService) : ControladorBase
{
    private const string Vista = "transferencia";
    private const string Flash = "comprobanteTransferencia";

    [HttpGet("")]
    public IActionResult Index()
    {
        Dictionary<string, string>? recibo = TomarFlash(Flash);
        if (recibo is not null)
            return View(Vista, new OperacionVista { Vista = OperacionVista.Comprobante, Recibo = recibo });

        string? cuentaCliente = CuentaCliente;
        return View(Vista, new OperacionVista { Origen = cuentaCliente, EsCliente = cuentaCliente is not null });
    }

    [HttpPost("")]
    public async Task<IActionResult> Enviar(string? paso, string? origen, string? destino, string? importe, string? clave)
    {
        // A Cliente can only transfer from their own account: the form value is ignored
        string? cuentaCliente = CuentaCliente;
        bool esCliente = cuentaCliente is not null;
        if (esCliente)
            origen = cuentaCliente;

        if (paso == "editar")
            return Formulario(origen, destino, importe, esCliente);

        // Always validated, also on confirm: hidden fields can be tampered with
        MovimientoForm form = MovimientoForm.Transferencia(origen, destino, importe);
        if (!form.EsValido)
            return Formulario(origen, destino, importe, esCliente, errores: form.Errores);

        if (paso != "confirmar")
            return Confirmacion(form, esCliente);

        string claveEnviada = "";
        if (!esCliente)
        {
            // Key of the source account, sent as typed (no trim)
            if (string.IsNullOrWhiteSpace(clave))
                return Confirmacion(form, esCliente, new Dictionary<string, string> { ["clave"] = RetiroController.ClaveVacia });
            claveEnviada = clave;
        }

        MovimientoResultado resultado = await movimientoService.RegistrarTransferenciaAsync(
            Token, form.Origen, form.Destino, form.Importe!.Value, claveEnviada);
        if (resultado.SesionExpirada)
            return SesionExpirada();
        if (!resultado.Ok)
            return Formulario(origen, destino, importe, esCliente, error: resultado.Mensaje);

        // Never the key
        Dictionary<string, string> recibo = Recibo("Transferencia", resultado, form.ImporteFormateado);
        recibo["origen"] = form.Origen;
        recibo["destino"] = form.Destino;
        GuardarFlash(Flash, recibo);

        // Post/Redirect/Get: a browser refresh never repeats the transfer
        return Redirect("~/transferencia");
    }

    // Form state keeping what the user typed
    private ViewResult Formulario(string? origen, string? destino, string? importe, bool esCliente,
        IReadOnlyDictionary<string, string>? errores = null, string? error = null)
        => View(Vista, new OperacionVista
        {
            Origen = origen,
            Destino = destino,
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
            Origen = form.Origen,
            Destino = form.Destino,
            Importe = form.ImporteTexto,
            ImporteFormateado = form.ImporteFormateado,
            EsCliente = esCliente,
            Errores = errores ?? new Dictionary<string, string>(),
        });
}
