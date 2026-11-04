using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Data;

namespace MOV_EUREKABANK_DOTNET_SOAP.Pruebas;

public static class PruebasConsultaMovimientos
{
    private static readonly Cuenta Cuenta = new("00100001", "Ana Torres", "USD", 540m, "ACTIVO");

    private static Movimiento Mov(string numero) =>
        new(numero, "2026-11-04 21:20", "Depósito", Movimiento.AccionIngreso, 10m, "");

    private static string Numeros(IEnumerable<Movimiento> movimientos) =>
        string.Join(",", movimientos.Select(m => m.Numero));

    public static Ejecutor Crear()
    {
        var e = new Ejecutor();

        e.Caso("El último movimiento va primero", () =>
        {
            var consulta = new ConsultaMovimientos(Cuenta, [Mov("3"), Mov("6"), Mov("7"), Mov("8"), Mov("9")]);
            Afirmar.Igual("9,8,7,6,3", Numeros(consulta.MovimientosRecientes));
            Afirmar.Igual("3,6,7,8,9", Numeros(consulta.Movimientos));
        });
        e.Caso("El orden no depende de la respuesta del servidor", () =>
        {
            var consulta = new ConsultaMovimientos(Cuenta, [Mov("10"), Mov("2"), Mov("9")]);
            Afirmar.Igual("10,9,2", Numeros(consulta.MovimientosRecientes));
        });

        return e;
    }
}
