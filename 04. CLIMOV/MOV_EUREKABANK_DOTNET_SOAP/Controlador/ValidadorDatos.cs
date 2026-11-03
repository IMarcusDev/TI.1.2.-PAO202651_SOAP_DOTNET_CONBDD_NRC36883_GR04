using System.Globalization;
using System.Text.RegularExpressions;

namespace MOV_EUREKABANK_DOTNET_SOAP.Controlador;

/// <summary>Validación de formato en el cliente (ERS 9.1.1 y ECUD). El servidor repite todas las reglas.</summary>
public static partial class ValidadorDatos
{
    public const int LargoCuenta = 8;
    public const int LargoClaveCuenta = 6;
    public const int MaxUsuario = 50;
    public const int MaxClaveEmpleado = 64;
    public const int MaxEnterosImporte = 10;
    public const int MaxImporte = MaxEnterosImporte + 3;

    public const string IngreseUsuario = "Ingrese el usuario";
    public const string IngreseCuenta = "Ingrese el número de cuenta";
    public const string IngreseClave = "Ingrese la clave";
    public const string IngreseImporte = "Ingrese el importe";
    public const string CuentaInvalida = "El número de cuenta debe tener 8 dígitos";
    public const string ClaveInvalida = "La clave de la cuenta debe tener 6 dígitos";
    public const string ImporteInvalido = "El importe debe ser mayor que cero y tener como máximo dos decimales";
    public const string CuentasIguales = "La cuenta de origen y la de destino deben ser diferentes";

    [GeneratedRegex("^[0-9]{8}$")]
    private static partial Regex PatronCuenta();

    [GeneratedRegex("^[0-9]{6}$")]
    private static partial Regex PatronClaveCuenta();

    [GeneratedRegex("^[0-9]{1,10}([.,][0-9]{0,2})?$")]
    private static partial Regex PatronImporte();

    public static string Limpiar(string? texto) => texto?.Trim() ?? "";

    /// <returns>El mensaje de error, o null si la cuenta es válida.</returns>
    public static string? ErrorCuenta(string cuenta)
    {
        if (cuenta.Length == 0)
            return IngreseCuenta;
        return PatronCuenta().IsMatch(cuenta) ? null : CuentaInvalida;
    }

    public static string? ErrorClaveCuenta(string clave)
    {
        if (clave.Length == 0)
            return IngreseClave;
        return PatronClaveCuenta().IsMatch(clave) ? null : ClaveInvalida;
    }

    public static string? ErrorImporte(string importe)
    {
        if (importe.Length == 0)
            return IngreseImporte;
        return ConvertirImporte(importe) is null ? ImporteInvalido : null;
    }

    /// <summary>Acepta punto o coma como separador decimal.</summary>
    /// <returns>El importe con dos decimales, o null si no es válido.</returns>
    public static decimal? ConvertirImporte(string? texto)
    {
        string limpio = Limpiar(texto);
        if (!PatronImporte().IsMatch(limpio))
            return null;

        decimal valor = decimal.Parse(limpio.Replace(',', '.').TrimEnd('.'), NumberStyles.AllowDecimalPoint,
                                      CultureInfo.InvariantCulture);
        // Fija la escala en dos decimales (10 -> 10.00), igual que BigDecimal.setScale(2).
        valor = decimal.Round(valor, 2) + 0.00m;
        return valor > 0 ? valor : null;
    }
}
