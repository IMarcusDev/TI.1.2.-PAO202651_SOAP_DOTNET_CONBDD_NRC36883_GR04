namespace ec.edu.monster.controladores;

/// <summary>Data of the Depósito / Retiro / Transferencia views.</summary>
public class OperacionVista
{
    public const string Formulario = "formulario";
    public const string Confirmacion = "confirmacion";
    public const string Comprobante = "comprobante";

    /// <summary>State of the screen: formulario | confirmacion | comprobante.</summary>
    public string Vista { get; init; } = Formulario;

    public string? Cuenta { get; init; }
    public string? Origen { get; init; }
    public string? Destino { get; init; }
    public string? Importe { get; init; }
    public string? ImporteFormateado { get; init; }

    /// <summary>Validation messages per field (cuenta, origen, destino, importe, clave).</summary>
    public IReadOnlyDictionary<string, string> Errores { get; init; } = new Dictionary<string, string>();

    /// <summary>Server message after a failed operation.</summary>
    public string? Error { get; init; }

    /// <summary>Receipt values (operacion, cuenta / origen / destino, importe, idMovimiento, nuevoSaldo, fecha, mensaje).</summary>
    public IReadOnlyDictionary<string, string> Recibo { get; init; } = new Dictionary<string, string>();

    /// <summary>A Cliente works on their own account: the account field is read-only and no key is asked.</summary>
    public bool EsCliente { get; init; }

    public string? ErrorDe(string campo) => Errores.TryGetValue(campo, out string? mensaje) ? mensaje : null;

    public string? ValorRecibo(string clave) => Recibo.TryGetValue(clave, out string? valor) ? valor : null;
}

/// <summary>One text input of an operation form (partial _Campo).</summary>
public record CampoVista(string Id, string Etiqueta, string? Valor, string Placeholder, string? Error)
{
    public const string AyudaImporte = "Mayor que 0, máximo 2 decimales";

    public string? Ayuda { get; init; }
    public bool SoloLectura { get; init; }
    public int? MaxLength { get; init; } = MovimientoForm.MaxCuenta;
    public string? InputMode { get; init; }

    /// <summary>The amount input: decimal keyboard, no length limit, hint under it.</summary>
    public static CampoVista ImporteDe(OperacionVista vista)
        => new("importe", "Importe", vista.Importe, "0.00", vista.ErrorDe("importe"))
        {
            Ayuda = AyudaImporte,
            MaxLength = null,
            InputMode = "decimal",
        };
}

/// <summary>Receipt rows (label, receipt key) in display order; missing values are skipped (partial _Recibo).</summary>
public record ReciboVista(OperacionVista Vista, string RutaNueva, IReadOnlyList<(string Etiqueta, string Clave)> Filas);

/// <summary>Account key input of the confirmation step (partial _Clave).</summary>
public record ClaveVista(string Etiqueta, string? Error);
