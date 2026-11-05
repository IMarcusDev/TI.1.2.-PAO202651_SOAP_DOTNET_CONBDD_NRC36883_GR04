using System.Globalization;
using System.Text.Json;
using ec.edu.monster.modelos.dto;
using Microsoft.AspNetCore.Mvc;

namespace ec.edu.monster.controladores;

/// <summary>Helpers shared by the screens of a logged-in user.</summary>
public abstract class ControladorBase : Controller
{
    public const string RutaExpirada = "~/login?expirada=1";

    /// <summary>Session token; AuthFilter already checked that it exists.</summary>
    protected string Token => SesionUsuario.TokenDe(HttpContext.Session) ?? "";

    /// <summary>Account stored at login for a Cliente, or null for an employee.</summary>
    protected string? CuentaCliente => SesionUsuario.CuentaCliente(HttpContext.Session);

    /// <summary>The server rejected the token: end the local session and go back to the login.</summary>
    protected IActionResult SesionExpirada()
    {
        HttpContext.Session.Clear();
        return Redirect(RutaExpirada);
    }

    /// <summary>Receipt of a successful operation, ready for the flash.</summary>
    protected static Dictionary<string, string> Recibo(string operacion, MovimientoResultado resultado, string importe)
    {
        var recibo = new Dictionary<string, string>
        {
            ["operacion"] = operacion,
            ["importe"] = importe,
            ["idMovimiento"] = resultado.NumeroMovimiento,
            ["fecha"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture),
            ["mensaje"] = resultado.Mensaje,
        };
        if (resultado.NuevoSaldo is not null)
            recibo["nuevoSaldo"] = Formatos.Dinero(resultado.NuevoSaldo);
        return recibo;
    }

    /// <summary>Saves the receipt so it survives exactly one redirect (Post/Redirect/Get).</summary>
    protected void GuardarFlash(string clave, Dictionary<string, string> recibo)
        => HttpContext.Session.SetString(clave, JsonSerializer.Serialize(recibo));

    /// <summary>Reads and removes the receipt; null when there is none.</summary>
    protected Dictionary<string, string>? TomarFlash(string clave)
    {
        string? json = HttpContext.Session.GetString(clave);
        if (json is null)
            return null;
        HttpContext.Session.Remove(clave);
        return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
    }
}
