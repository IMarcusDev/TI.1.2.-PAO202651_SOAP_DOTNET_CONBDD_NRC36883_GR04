using MOV_EUREKABANK_DOTNET_SOAP.Utils;

namespace MOV_EUREKABANK_DOTNET_SOAP.Pruebas;

public static class PruebasFormato
{
    public static Ejecutor Crear()
    {
        var e = new Ejecutor();

        e.Caso("Importe con dos decimales y moneda", () =>
        {
            Afirmar.Igual("1.250,50 USD", Formato.Importe(1250.5m, "USD"));
            Afirmar.Igual("0,00", Formato.Importe(0m));
            Afirmar.Igual(Formato.SinDato, Formato.Importe(null, "USD"));
        });
        e.Caso("Fecha del servidor como día/mes/año hora:minuto", () =>
        {
            Afirmar.Igual("31/10/2026 16:40", Formato.Fecha("2026-10-31T16:40:12.345-05:00"));
            Afirmar.Igual("31/10/2026 16:40", Formato.Fecha("2026-10-31 16:40:12"));
            Afirmar.Igual("31/10/2026", Formato.Fecha("2026-10-31"));
            Afirmar.Igual("31/10/2026 16:40", Formato.Fecha("31/10/2026 16:40"));
            Afirmar.Igual(Formato.SinDato, Formato.Fecha(""));
        });

        return e;
    }
}
