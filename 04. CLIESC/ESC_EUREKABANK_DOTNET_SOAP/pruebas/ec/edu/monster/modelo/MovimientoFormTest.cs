using ec.edu.monster.modelo;
using ec.edu.monster.modelo.dto;

namespace ec.edu.monster.modelo;

public class MovimientoFormTest
{
    [Fact]
    public void RetiroDeClienteUsesTheAccountAndTheAmountWithoutAKey()
    {
        MovimientoForm form = MovimientoForm.Retiro(" 00100001 ", "120,50");

        Assert.True(form.EsValido);
        Assert.Equal("00100001", form.Cuenta);
        Assert.Equal("", form.Clave);
        Assert.Equal(120.50m, form.Importe);
        Assert.Equal("$ 120.50", form.ImporteFormateado);
    }

    [Fact]
    public void RetiroDeClienteRequiresTheAccount()
    {
        MovimientoForm form = MovimientoForm.Retiro("  ", "10");

        Assert.False(form.EsValido);
        Assert.Equal("Ingrese el número de cuenta", form.Errores["cuenta"]);
    }

    [Fact]
    public void RetiroDeEmpleadoRequiresTheKeyAndSendsItAsTyped()
    {
        Assert.Equal(MovimientoForm.ClaveVacia, MovimientoForm.RetiroEmpleado("1", "  ", "10").Errores["clave"]);
        Assert.Equal(" clave ", MovimientoForm.RetiroEmpleado("1", " clave ", "10").Clave);
    }

    [Fact]
    public void DepositoRequiresTheDestinationAccountAndTheAmount()
    {
        MovimientoForm form = MovimientoForm.Deposito("", "");

        Assert.False(form.EsValido);
        Assert.Equal("Ingrese la cuenta de destino", form.Errores["cuenta"]);
        Assert.Equal(MovimientoForm.ImporteVacio, form.Errores["importe"]);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("0.00")]
    [InlineData("0.001")]
    [InlineData("abc")]
    [InlineData("12,3,4")]
    [InlineData("12345678901")]
    public void RejectsInvalidAmounts(string importe)
        => Assert.Equal(MovimientoForm.ImporteInvalido,
            MovimientoForm.Deposito("1", importe).Errores["importe"]);

    [Fact]
    public void ATooLongAccountIsRejected()
        => Assert.Equal(MovimientoForm.CuentaLarga,
            MovimientoForm.Deposito(new string('1', 31), "10").Errores["cuenta"]);

    [Fact]
    public void ATransferBetweenTheSameAccountIsRejected()
    {
        MovimientoForm form = MovimientoForm.TransferenciaEmpleado("001", "001", "k", "10");

        Assert.False(form.EsValido);
        Assert.Equal(MovimientoForm.CuentasIguales, form.Errores["destino"]);
    }

    [Fact]
    public void ATransferDeClienteSendsNoKey()
    {
        MovimientoForm form = MovimientoForm.Transferencia("001", "002", "10");

        Assert.True(form.EsValido);
        Assert.Equal("", form.Clave);
    }
}

public class ComprobanteTest
{
    [Fact]
    public void ATransferShowsBothAccountsAndTheFormattedAmount()
    {
        MovimientoForm form = MovimientoForm.TransferenciaEmpleado("001", "002", "k", "99.99");

        Comprobante comprobante = Comprobante.Transferencia(form, new MovimientoResultado(true, "ok", "9", 400m));

        Assert.Equal("Transferencia", comprobante.Operacion);
        Assert.Equal("001", comprobante.CuentaOrigen);
        Assert.Equal("002", comprobante.CuentaDestino);
        Assert.Equal("$ 99.99", comprobante.Importe);
        Assert.Equal("000009", comprobante.NumeroMovimiento);
        Assert.Equal("$ 400.00", comprobante.NuevoSaldo);
        Assert.Equal(10, comprobante.Fecha.Length);
        Assert.Equal(
            ["Operación", "Cuenta de origen", "Cuenta de destino", "Importe", "N.º de movimiento", "Nuevo saldo", "Fecha", "Mensaje"],
            comprobante.Filas().Select(f => f.Etiqueta));
    }

    [Fact]
    public void ADepositHidesTheOriginAndARetiroHidesTheDestination()
    {
        MovimientoForm deposito = MovimientoForm.Deposito("001", "10");
        MovimientoForm retiro = MovimientoForm.Retiro("001", "10");
        var resultado = new MovimientoResultado(true, "ok", "1", 10m);

        Assert.DoesNotContain(Comprobante.Deposito(deposito, resultado).Filas(), f => f.Etiqueta == "Cuenta de origen");
        Assert.DoesNotContain(Comprobante.Retiro(retiro, resultado).Filas(), f => f.Etiqueta == "Cuenta de destino");
    }
}
