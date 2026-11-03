using MOV_EUREKABANK_DOTNET_SOAP.Controlador;

namespace MOV_EUREKABANK_DOTNET_SOAP.Pruebas;

public static class PruebasValidador
{
    public static Ejecutor Crear()
    {
        var e = new Ejecutor();

        e.Caso("Cuenta: exige ocho dígitos", () =>
        {
            Afirmar.Nulo(ValidadorDatos.ErrorCuenta("00100001"));
            Afirmar.Igual("Ingrese el número de cuenta", ValidadorDatos.ErrorCuenta(""));
            Afirmar.Igual("El número de cuenta debe tener 8 dígitos", ValidadorDatos.ErrorCuenta("1234567"));
            Afirmar.Igual("El número de cuenta debe tener 8 dígitos", ValidadorDatos.ErrorCuenta("1234567a"));
        });
        e.Caso("Clave de cuenta: exige seis dígitos", () =>
        {
            Afirmar.Nulo(ValidadorDatos.ErrorClaveCuenta("123456"));
            Afirmar.Igual("Ingrese la clave", ValidadorDatos.ErrorClaveCuenta(""));
            Afirmar.Igual("La clave de la cuenta debe tener 6 dígitos", ValidadorDatos.ErrorClaveCuenta("12345"));
        });
        e.Caso("Importe: acepta punto o coma y hasta dos decimales", () =>
        {
            Afirmar.Igual("150.50", ValidadorDatos.ConvertirImporte("150,5")?.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Afirmar.Igual(150.25m, ValidadorDatos.ConvertirImporte("150.25"));
            Afirmar.Igual("10.00", ValidadorDatos.ConvertirImporte("10")?.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Afirmar.Verdadero(ValidadorDatos.ConvertirImporte("9999999999.99") is not null, "10 enteros y 2 decimales");
        });
        e.Caso("Importe: rechaza cero, más de dos decimales y más de diez enteros", () =>
        {
            Afirmar.Nulo(ValidadorDatos.ConvertirImporte("0"));
            Afirmar.Nulo(ValidadorDatos.ConvertirImporte("0,00"));
            Afirmar.Nulo(ValidadorDatos.ConvertirImporte("1.234"));
            Afirmar.Nulo(ValidadorDatos.ConvertirImporte("12345678901"));
            Afirmar.Nulo(ValidadorDatos.ConvertirImporte("-5"));
            Afirmar.Nulo(ValidadorDatos.ConvertirImporte("abc"));
            Afirmar.Igual("Ingrese el importe", ValidadorDatos.ErrorImporte(""));
            Afirmar.Igual("El importe debe ser mayor que cero y tener como máximo dos decimales", ValidadorDatos.ErrorImporte("0"));
        });

        return e;
    }
}
