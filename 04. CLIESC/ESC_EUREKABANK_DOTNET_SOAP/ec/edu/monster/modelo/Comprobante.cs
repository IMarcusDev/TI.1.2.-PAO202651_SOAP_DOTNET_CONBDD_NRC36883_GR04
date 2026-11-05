using System.Globalization;
using ec.edu.monster.modelo.dto;

namespace ec.edu.monster.modelo;

/// <summary>Everything the receipt shows after a deposit, withdrawal or transfer.</summary>
public sealed record Comprobante(
    string Operacion,
    string CuentaOrigen,
    string CuentaDestino,
    string Importe,
    string NumeroMovimiento,
    string NuevoSaldo,
    string Fecha,
    string Mensaje)
{
    public static Comprobante Deposito(MovimientoForm form, MovimientoResultado resultado)
        => new("Depósito", "", form.Cuenta, form.ImporteFormateado,
            Formatos.NumeroMovimiento(resultado.NumeroMovimiento), Formatos.Dinero(resultado.NuevoSaldo),
            Hoy(), resultado.Mensaje);

    public static Comprobante Retiro(MovimientoForm form, MovimientoResultado resultado)
        => new("Retiro", form.Cuenta, "", form.ImporteFormateado,
            Formatos.NumeroMovimiento(resultado.NumeroMovimiento), Formatos.Dinero(resultado.NuevoSaldo),
            Hoy(), resultado.Mensaje);

    public static Comprobante Transferencia(MovimientoForm form, MovimientoResultado resultado)
        => new("Transferencia", form.Origen, form.Destino, form.ImporteFormateado,
            Formatos.NumeroMovimiento(resultado.NumeroMovimiento), Formatos.Dinero(resultado.NuevoSaldo),
            Hoy(), resultado.Mensaje);

    /// <summary>The rows the dialog shows, in order; optional rows appear only when they apply.</summary>
    public IReadOnlyList<FilaComprobante> Filas()
    {
        var filas = new List<FilaComprobante> { new("Operación", Operacion) };
        if (!string.IsNullOrWhiteSpace(CuentaOrigen))
            filas.Add(new FilaComprobante("Cuenta de origen", CuentaOrigen));
        if (!string.IsNullOrWhiteSpace(CuentaDestino))
            filas.Add(new FilaComprobante("Cuenta de destino", CuentaDestino));
        filas.Add(new FilaComprobante("Importe", Importe));
        filas.Add(new FilaComprobante("N.º de movimiento", NumeroMovimiento));
        if (!string.IsNullOrWhiteSpace(NuevoSaldo))
            filas.Add(new FilaComprobante("Nuevo saldo", NuevoSaldo));
        filas.Add(new FilaComprobante("Fecha", Fecha));
        filas.Add(new FilaComprobante("Mensaje", Mensaje));
        return filas;
    }

    private static string Hoy() => DateTime.Now.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
}

public sealed record FilaComprobante(string Etiqueta, string Valor);
