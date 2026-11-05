using ec.edu.monster.modelo.dto;

namespace ec.edu.monster.modelo.dto;

public class FormatosTest
{
    [Theory]
    [InlineData("2026-11-03T09:15:00", "03/11/2026 09:15")]
    [InlineData("2026-11-03T09:15", "03/11/2026 09:15")]
    [InlineData("31/10/2026 16:40", "31/10/2026 16:40")]
    [InlineData("", "")]
    public void FechaFormatsIsoDatesAndKeepsAnythingElse(string entrada, string esperado)
        => Assert.Equal(esperado, Formatos.Fecha(entrada));

    [Fact]
    public void DineroUsesDollarSignThousandsAndTwoDecimals()
    {
        Assert.Equal("$ 1,250.50", Formatos.Dinero(1250.5m));
        Assert.Equal("", Formatos.Dinero(null));
    }

    [Theory]
    [InlineData("6", "000006")]
    [InlineData("abc", "abc")]
    public void NumeroMovimientoUsesSixDigits(string entrada, string esperado)
        => Assert.Equal(esperado, Formatos.NumeroMovimiento(entrada));

    [Fact]
    public void ReferenciaShowsADashWhenEmpty() => Assert.Equal("-", Formatos.Referencia(" "));

    [Theory]
    [InlineData("379.50", 379.50)]
    [InlineData("379,50", 379.50)]
    [InlineData("", null)]
    [InlineData("abc", null)]
    public void DecimalReadsTheServerNumber(string texto, double? esperado)
        => Assert.Equal((decimal?)esperado, Formatos.Decimal(texto));
}

public class HistorialCuentaTest
{
    private static MovimientoConsultado Fila(string numero, string tipo = "Depósito")
        => new(numero, "", tipo, "INGRESO", 1m, "-");

    [Fact]
    public void OrdersNewestFirstWithUnreadableNumbersLastInReverseArrivalOrder()
    {
        var historial = new HistorialCuenta(true, "", null,
            [Fila("000002"), Fila("x", "A"), Fila("000010"), Fila("y", "B"), Fila("000001")]);

        Assert.Equal(["000010", "000002", "000001", "y", "x"], historial.Movimientos.Select(m => m.Numero));
    }

    [Fact]
    public void AnErrorHasNoAccountAndNoRows()
    {
        HistorialCuenta error = HistorialCuenta.Error("La cuenta no existe");

        Assert.False(error.Ok);
        Assert.Null(error.Cuenta);
        Assert.True(error.SinMovimientos);
    }
}
