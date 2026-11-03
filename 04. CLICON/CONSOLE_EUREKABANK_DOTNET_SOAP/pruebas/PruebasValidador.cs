//revisado y aprovado por Marcos Escobar
using CONSOLE_EUREKABANK_DOTNET_SOAP.Controllers;
using CONSOLE_EUREKABANK_DOTNET_SOAP.Views.Utils;

namespace CONSOLE_EUREKABANK_DOTNET_SOAP.Pruebas;

internal static class PruebasValidador
{
    public static Ejecutor Crear()
    {
        var e = new Ejecutor();

        e.Caso("Cuenta vacía, corta o con letras", () =>
        {
            Afirmar.Igual(ValidadorDatos.IngreseCuenta, ValidadorDatos.ErrorCuenta(""));
            Afirmar.Igual(ValidadorDatos.CuentaInvalida, ValidadorDatos.ErrorCuenta("0010001"));
            Afirmar.Igual(ValidadorDatos.CuentaInvalida, ValidadorDatos.ErrorCuenta("0010000A"));
            Afirmar.Nulo(ValidadorDatos.ErrorCuenta("00100001"), "cuenta válida");
        });

        e.Caso("Clave de cuenta de 6 dígitos", () =>
        {
            Afirmar.Igual(ValidadorDatos.IngreseClave, ValidadorDatos.ErrorClaveCuenta(""));
            Afirmar.Igual(ValidadorDatos.ClaveInvalida, ValidadorDatos.ErrorClaveCuenta("12345"));
            Afirmar.Nulo(ValidadorDatos.ErrorClaveCuenta("123456"), "clave válida");
        });

        e.Caso("Importe con coma o punto y dos decimales", () =>
        {
            Afirmar.Igual(10.50m, ValidadorDatos.ConvertirImporte("10,5"));
            Afirmar.Igual(120.50m, ValidadorDatos.ConvertirImporte("120.50"));
            Afirmar.Nulo(ValidadorDatos.ConvertirImporte("10.555"), "tres decimales");
            Afirmar.Nulo(ValidadorDatos.ConvertirImporte("0"), "cero");
            Afirmar.Nulo(ValidadorDatos.ConvertirImporte("-5"), "negativo");
            Afirmar.Igual(ValidadorDatos.ImporteInvalido, ValidadorDatos.ErrorImporte("abc"));
        });

        e.Caso("Formato del ERS: importes y fechas", () =>
        {
            Afirmar.Igual("1.234,50 USD", Formato.Importe(1234.5m, "USD"));
            Afirmar.Igual("03/11/2026 16:07", Formato.Fecha("2026-11-03T16:07:05"));
            Afirmar.Igual("31/10/2026 16:40", Formato.Fecha("31/10/2026 16:40"));
        });

        return e;
    }
}
