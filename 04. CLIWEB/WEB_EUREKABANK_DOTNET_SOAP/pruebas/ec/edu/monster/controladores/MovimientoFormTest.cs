namespace ec.edu.monster.controladores;

public class MovimientoFormTest
{
    [Theory]
    [InlineData("120", 120)]
    [InlineData("120,5", 120.5)]
    [InlineData(" 0.01 ", 0.01)]
    [InlineData("9999999999.99", 9999999999.99)]
    public void AcceptsPositiveAmountsWithUpToTwoDecimals(string importe, double esperado)
    {
        MovimientoForm form = MovimientoForm.Retiro("00100001", importe);

        Assert.True(form.EsValido);
        Assert.Equal((decimal)esperado, form.Importe);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("1.234")]
    [InlineData("1.2.3")]
    [InlineData("1,2.3")]
    [InlineData("abc")]
    [InlineData("12345678901")]
    public void RejectsInvalidAmounts(string importe)
        => Assert.Equal(MovimientoForm.ImporteInvalido, MovimientoForm.Retiro("1", importe).Errores["importe"]);

    [Fact]
    public void EachFormNamesTheMissingFields()
    {
        Assert.Equal("Ingrese el número de cuenta", MovimientoForm.Retiro(" ", "1").Errores["cuenta"]);
        Assert.Equal("Ingrese la cuenta de destino", MovimientoForm.Deposito(null, "1").Errores["cuenta"]);
        Assert.Equal(MovimientoForm.ImporteVacio, MovimientoForm.Deposito("1", "").Errores["importe"]);

        MovimientoForm transferencia = MovimientoForm.Transferencia("", "", "1");
        Assert.Equal("Ingrese la cuenta de origen", transferencia.Errores["origen"]);
        Assert.Equal("Ingrese la cuenta de destino", transferencia.Errores["destino"]);
    }

    [Fact]
    public void RejectsAnAccountLongerThan30Characters()
        => Assert.Equal(MovimientoForm.CuentaLarga, MovimientoForm.Retiro(new string('1', 31), "1").Errores["cuenta"]);

    [Fact]
    public void ATransferNeedsTwoDifferentAccounts()
        => Assert.Equal(MovimientoForm.CuentasIguales, MovimientoForm.Transferencia("001", " 001 ", "1").Errores["destino"]);

    [Fact]
    public void TrimsTheAccountsAndFormatsTheAmount()
    {
        MovimientoForm form = MovimientoForm.Transferencia(" 001 ", "002", "1250.5");

        Assert.Equal(("001", "002", "$ 1,250.50", "1250.5"), (form.Origen, form.Destino, form.ImporteFormateado, form.ImporteTexto));
    }
}
