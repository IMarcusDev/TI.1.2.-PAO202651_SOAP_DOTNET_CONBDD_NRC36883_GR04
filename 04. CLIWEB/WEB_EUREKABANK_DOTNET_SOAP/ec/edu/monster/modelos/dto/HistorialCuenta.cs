using System.Globalization;

namespace ec.edu.monster.modelos.dto;

/// <summary>
/// Answer of "Consultar movimientos": account details and movement history, already formatted
/// for the view. Built by MovimientoService from the SOAP response.
/// </summary>
public class HistorialCuenta
{
    public HistorialCuenta(bool ok, string mensaje, CuentaConsultada? cuenta,
        IEnumerable<MovimientoConsultado>? movimientos, bool sesionExpirada = false)
    {
        Ok = ok;
        Mensaje = mensaje;
        Cuenta = cuenta;
        SesionExpirada = sesionExpirada;
        Movimientos = RecientesPrimero(movimientos ?? []);
    }

    public bool Ok { get; }

    public string Mensaje { get; }

    /// <summary>The server rejected the token: the session must end.</summary>
    public bool SesionExpirada { get; }

    public CuentaConsultada? Cuenta { get; }

    public IReadOnlyList<MovimientoConsultado> Movimientos { get; }

    public bool SinMovimientos => Movimientos.Count == 0;

    /// <summary>Failed query (server error or connection problem): no account, no rows.</summary>
    public static HistorialCuenta Error(string mensaje, bool sesionExpirada = false)
        => new(false, mensaje, null, [], sesionExpirada);

    /// <summary>
    /// Newest first, which is the order every screen shows. The movement number is the server's
    /// global ID, which grows with every record, so the largest one is the newest. It does not depend
    /// on the response order; numbers that cannot be read go last, in reverse arrival order.
    /// </summary>
    private static List<MovimientoConsultado> RecientesPrimero(IEnumerable<MovimientoConsultado> movimientos)
    {
        List<MovimientoConsultado> copia = movimientos.Reverse().ToList();
        // OrderByDescending is stable: equal keys keep the reversed arrival order.
        return copia.OrderByDescending(NumeroDe).ToList();
    }

    private static long NumeroDe(MovimientoConsultado movimiento)
        => long.TryParse(movimiento.Numero.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out long numero)
            ? numero
            : long.MinValue;
}

/// <summary>Account data shown above the table.</summary>
public record CuentaConsultada(string Numero, string Titular, string Moneda, decimal? Saldo, string Estado)
{
    public string SaldoFormateado => Formatos.Dinero(Saldo);
}

/// <summary>One movement row: number, date and reference already formatted, amount as a decimal.</summary>
public record MovimientoConsultado(string Numero, string Fecha, string Tipo, string Accion, decimal? Importe, string Referencia)
{
    /// <summary>INGRESO is styled as a positive movement; the server sends INGRESO / SALIDA.</summary>
    public bool EsIngreso => Accion == "INGRESO";

    public string ImporteFormateado => Formatos.Dinero(Importe);
}
