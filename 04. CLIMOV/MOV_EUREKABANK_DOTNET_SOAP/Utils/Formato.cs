using System.Globalization;
using System.Text.RegularExpressions;

namespace MOV_EUREKABANK_DOTNET_SOAP.Utils;

/// <summary>RNF-009: importes con dos decimales y la moneda de la cuenta; fechas con el formato dd/MM/yyyy HH:mm.</summary>
public static partial class Formato
{
    public const string SinDato = "—";

    private static readonly NumberFormatInfo Simbolos = new()
    {
        NumberDecimalSeparator = ",",
        NumberGroupSeparator = ".",
        NegativeSign = "-",
    };

    [GeneratedRegex(@"^([0-9]{4})-([0-9]{2})-([0-9]{2})(?:[T ]([0-9]{2}):([0-9]{2}))?.*$")]
    private static partial Regex FechaIso();

    public static string Importe(decimal? valor)
        => valor is decimal v ? Math.Round(v, 2, MidpointRounding.AwayFromZero).ToString("#,##0.00", Simbolos) : SinDato;

    public static string Importe(decimal? valor, string? moneda)
    {
        string texto = Importe(valor);
        return valor is null || string.IsNullOrWhiteSpace(moneda) ? texto : $"{texto} {moneda.Trim()}";
    }

    /// <summary>Muestra la fecha del servidor con el formato del ERS; si no la reconoce, la deja como llegó.</summary>
    public static string Fecha(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return SinDato;

        Match iso = FechaIso().Match(texto.Trim());
        if (!iso.Success)
            return texto.Trim();

        string fecha = $"{iso.Groups[3].Value}/{iso.Groups[2].Value}/{iso.Groups[1].Value}";
        return iso.Groups[4].Success ? $"{fecha} {iso.Groups[4].Value}:{iso.Groups[5].Value}" : fecha;
    }

    public static string Fecha(DateTime fecha) => fecha.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
}
