using System.Globalization;

namespace ec.edu.monster.modelos.dto;

/// <summary>
/// Display formats of the UI template: dates as dd/MM/yyyy HH:mm, amounts with two decimals
/// and a dollar sign, movement numbers with six digits.
/// </summary>
public static class Formatos
{
    private static readonly string[] FormatosIso =
        ["yyyy-MM-dd'T'HH:mm:ss", "yyyy-MM-dd'T'HH:mm", "yyyy-MM-dd'T'HH:mm:ss.FFFFFFF"];

    /// <summary>Server date (ISO, e.g. 2026-11-03T09:15:00) as 03/11/2026 09:15; anything else unchanged.</summary>
    public static string Fecha(string? iso)
    {
        if (string.IsNullOrWhiteSpace(iso))
            return "";
        return DateTime.TryParseExact(iso.Trim(), FormatosIso, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fecha)
            ? fecha.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)
            : iso;
    }

    /// <summary>Dollar amount, e.g. $ 1,250.50 (US style, always 2 decimals); empty when missing.</summary>
    public static string Dinero(decimal? valor)
        => valor is null ? "" : "$ " + valor.Value.ToString("#,##0.00", CultureInfo.InvariantCulture);

    /// <summary>Reference account of a movement, or "-" when it has none.</summary>
    public static string Referencia(string? cuenta)
        => string.IsNullOrWhiteSpace(cuenta) ? "-" : cuenta;

    /// <summary>Movement number with six digits, e.g. 000006; a value that is not a number stays as it is.</summary>
    public static string NumeroMovimiento(string? numero)
    {
        if (string.IsNullOrWhiteSpace(numero))
            return "";
        return int.TryParse(numero.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out int valor)
            ? valor.ToString("D6", CultureInfo.InvariantCulture)
            : numero.Trim();
    }

    /// <summary>Decimal text from the server ("379.50"); null when missing or unreadable.</summary>
    public static decimal? Decimal(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return null;
        return decimal.TryParse(texto.Trim().Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal valor)
            ? valor
            : null;
    }
}
