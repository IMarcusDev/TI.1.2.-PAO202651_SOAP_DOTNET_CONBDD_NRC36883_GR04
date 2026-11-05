using System.Globalization;
using System.Text.RegularExpressions;
using ec.edu.monster.modelo.dto;

namespace ec.edu.monster.modelo;

/// <summary>
/// Validation of the withdrawal / deposit / transfer forms. Same rules and messages as the web
/// client. Errors are kept per field name (cuenta, origen, destino, importe, clave) so the view
/// can show each message under its input.
/// </summary>
public sealed class MovimientoForm
{
    // Account ids are varchar(30) in the database.
    public const int MaxCuenta = 30;

    // Up to 10 integer digits and at most 2 decimals, "." as separator (after normalizing ",").
    private static readonly Regex ImportePatron = new(@"^\d{1,10}(\.\d{1,2})?$", RegexOptions.Compiled);

    public const string ImporteVacio = "Ingrese el importe";
    public const string ImporteInvalido = "El importe debe ser mayor que cero y tener como máximo dos decimales";
    public const string CuentaLarga = "La cuenta debe tener como máximo 30 caracteres";
    public const string CuentasIguales = "La cuenta de origen y la de destino deben ser diferentes";
    public const string ClaveVacia = "Ingrese la clave de la cuenta";

    private readonly Dictionary<string, string> _errores = new();

    private MovimientoForm()
    {
    }

    public string Cuenta { get; private set; } = "";

    public string Origen { get; private set; } = "";

    public string Destino { get; private set; } = "";

    /// <summary>Account key of an Empleado, or "" for a Cliente.</summary>
    public string Clave { get; private set; } = "";

    /// <summary>Parsed amount, or null when it is invalid.</summary>
    public decimal? Importe { get; private set; }

    public IReadOnlyDictionary<string, string> Errores => _errores;

    public bool EsValido => _errores.Count == 0;

    public string ImporteFormateado => Formatos.Dinero(Importe);

    /// <summary>
    /// Withdrawal of a Cliente: only their own account and the amount; the server ties the token
    /// to the account, so no key is sent.
    /// </summary>
    public static MovimientoForm Retiro(string cuenta, string importe)
        => Retiro(cuenta, "", importe, requiereClave: false);

    /// <summary>Withdrawal of an Empleado: any account, its key and the amount.</summary>
    public static MovimientoForm RetiroEmpleado(string cuenta, string clave, string importe)
        => Retiro(cuenta, clave, importe, requiereClave: true);

    /// <summary>Deposit: the server takes the employee from the token; only account and amount.</summary>
    public static MovimientoForm Deposito(string cuenta, string importe)
    {
        var form = new MovimientoForm();
        form.Cuenta = form.ValidarCuenta("cuenta", cuenta, "Ingrese la cuenta de destino");
        form.Importe = form.ValidarImporte(importe);
        return form;
    }

    /// <summary>
    /// Transfer of a Cliente: only the target account (the source is their own) and the amount;
    /// the server ties the token to the source account, so no key is sent.
    /// </summary>
    public static MovimientoForm Transferencia(string origen, string destino, string importe)
        => Transferencia(origen, destino, "", importe, requiereClave: false);

    /// <summary>Transfer of an Empleado: both accounts, the key of the origin and the amount.</summary>
    public static MovimientoForm TransferenciaEmpleado(string origen, string destino, string clave, string importe)
        => Transferencia(origen, destino, clave, importe, requiereClave: true);

    /// <summary>Formats an amount as "$ 1,250.00" (US style, always 2 decimals).</summary>
    public static string FormatearImporte(decimal valor)
        => "$ " + valor.ToString("#,##0.00", CultureInfo.InvariantCulture);

    private static MovimientoForm Retiro(string cuenta, string clave, string importe, bool requiereClave)
    {
        var form = new MovimientoForm();
        form.Cuenta = form.ValidarCuenta("cuenta", cuenta, "Ingrese el número de cuenta");
        form.Clave = form.ValidarClave(clave, requiereClave);
        form.Importe = form.ValidarImporte(importe);
        return form;
    }

    private static MovimientoForm Transferencia(string origen, string destino, string clave,
        string importe, bool requiereClave)
    {
        var form = new MovimientoForm();
        form.Origen = form.ValidarCuenta("origen", origen, "Ingrese la cuenta de origen");
        form.Destino = form.ValidarCuenta("destino", destino, "Ingrese la cuenta de destino");
        if (!form._errores.ContainsKey("origen") && !form._errores.ContainsKey("destino")
            && form.Origen == form.Destino)
        {
            form._errores["destino"] = CuentasIguales;
        }
        form.Clave = form.ValidarClave(clave, requiereClave);
        form.Importe = form.ValidarImporte(importe);
        return form;
    }

    // The key is sent as typed (the server compares it with the stored one): no trim.
    private string ValidarClave(string? valor, bool requerida)
    {
        if (!requerida)
            return "";
        string tecleada = valor ?? "";
        if (string.IsNullOrWhiteSpace(tecleada))
            _errores["clave"] = ClaveVacia;
        return tecleada;
    }

    // Returns the trimmed value (empty string when missing) and records the error, if any.
    private string ValidarCuenta(string campo, string? valor, string mensajeVacio)
    {
        string limpio = valor?.Trim() ?? "";
        if (limpio.Length == 0)
            _errores[campo] = mensajeVacio;
        else if (limpio.Length > MaxCuenta)
            _errores[campo] = CuentaLarga;
        return limpio;
    }

    private decimal? ValidarImporte(string? valor)
    {
        string limpio = valor?.Trim() ?? "";
        if (limpio.Length == 0)
        {
            _errores["importe"] = ImporteVacio;
            return null;
        }
        // Accept "," as decimal separator; the regex rejects mixed or repeated separators.
        string normalizado = limpio.Replace(',', '.');
        if (!ImportePatron.IsMatch(normalizado))
        {
            _errores["importe"] = ImporteInvalido;
            return null;
        }
        decimal numero = decimal.Parse(normalizado, CultureInfo.InvariantCulture);
        if (numero <= 0)
        {
            _errores["importe"] = ImporteInvalido;
            return null;
        }
        return numero;
    }
}
