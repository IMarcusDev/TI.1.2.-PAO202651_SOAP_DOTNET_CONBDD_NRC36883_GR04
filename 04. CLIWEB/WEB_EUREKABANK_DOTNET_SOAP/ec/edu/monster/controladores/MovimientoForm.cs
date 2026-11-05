using System.Globalization;
using System.Text.RegularExpressions;
using ec.edu.monster.modelos.dto;

namespace ec.edu.monster.controladores;

/// <summary>
/// Validation of the withdrawal / deposit / transfer forms (no MVC types).
/// Errors are kept per field name (cuenta, origen, destino, importe) so the view can show
/// each message under its input.
/// </summary>
public sealed partial class MovimientoForm
{
    /// <summary>Account ids are varchar(30) in the database.</summary>
    public const int MaxCuenta = 30;

    public const string ImporteVacio = "Ingrese el importe";
    public const string ImporteInvalido = "El importe debe ser mayor que cero y tener como máximo dos decimales";
    public const string CuentaLarga = "La cuenta debe tener como máximo 30 caracteres";
    public const string CuentasIguales = "La cuenta de origen y la de destino deben ser diferentes";

    private readonly Dictionary<string, string> _errores = [];

    private MovimientoForm()
    {
    }

    public string Cuenta { get; private set; } = "";
    public string Origen { get; private set; } = "";
    public string Destino { get; private set; } = "";

    /// <summary>Parsed amount, or null when it is invalid.</summary>
    public decimal? Importe { get; private set; }

    public bool EsValido => _errores.Count == 0;

    public IReadOnlyDictionary<string, string> Errores => _errores;

    public string ImporteFormateado => Formatos.Dinero(Importe);

    /// <summary>Amount as plain text for the hidden field of the confirmation (120.5).</summary>
    public string ImporteTexto => Importe?.ToString(CultureInfo.InvariantCulture) ?? "";

    /// <summary>Withdrawal: one account (the one the money leaves) and the amount.</summary>
    public static MovimientoForm Retiro(string? cuenta, string? importe)
    {
        var form = new MovimientoForm();
        form.Cuenta = form.ValidarCuenta("cuenta", cuenta, "Ingrese el número de cuenta");
        form.Importe = form.ValidarImporte(importe);
        return form;
    }

    /// <summary>Deposit: target account and the amount (the server takes the employee from the token).</summary>
    public static MovimientoForm Deposito(string? cuenta, string? importe)
    {
        var form = new MovimientoForm();
        form.Cuenta = form.ValidarCuenta("cuenta", cuenta, "Ingrese la cuenta de destino");
        form.Importe = form.ValidarImporte(importe);
        return form;
    }

    /// <summary>Transfer: source and target accounts (must differ) and the amount.</summary>
    public static MovimientoForm Transferencia(string? origen, string? destino, string? importe)
    {
        var form = new MovimientoForm();
        form.Origen = form.ValidarCuenta("origen", origen, "Ingrese la cuenta de origen");
        form.Destino = form.ValidarCuenta("destino", destino, "Ingrese la cuenta de destino");
        if (!form._errores.ContainsKey("origen") && !form._errores.ContainsKey("destino")
            && form.Origen == form.Destino)
        {
            form._errores["destino"] = CuentasIguales;
        }
        form.Importe = form.ValidarImporte(importe);
        return form;
    }

    // Returns the trimmed value (empty string when missing) and records the error, if any
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
        // Accept "," as decimal separator; the pattern rejects mixed or repeated separators
        string normalizado = limpio.Replace(',', '.');
        if (!PatronImporte().IsMatch(normalizado))
        {
            _errores["importe"] = ImporteInvalido;
            return null;
        }
        decimal numero = decimal.Parse(normalizado, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
        if (numero <= 0)
        {
            _errores["importe"] = ImporteInvalido;
            return null;
        }
        return numero;
    }

    // Up to 10 integer digits and at most 2 decimals, "." as separator (after normalizing ",")
    [GeneratedRegex(@"\A[0-9]{1,10}(\.[0-9]{1,2})?\z")]
    private static partial Regex PatronImporte();
}
