using ec.edu.monster.modelo;
using ec.edu.monster.vista.modelos;
using ec.edu.monster.fakes;

namespace ec.edu.monster.vista.modelos;

/// <summary>Menu texts and the card each one opens.</summary>
public class MenuViewModelTest
{
    [Fact]
    public void Empleado_puede_depositar_y_ve_los_textos_de_empleado()
    {
        var modelo = new MenuViewModel(Sesiones.Empleado(), _ => { });

        Assert.True(modelo.PuedeDepositar);
        Assert.Equal("Ana Pérez", modelo.Nombre);
        Assert.Equal("Registre un depósito en la cuenta de un cliente", modelo.DepositoDetalle);
        Assert.Equal("Registre un retiro de la cuenta de un cliente", modelo.RetiroDetalle);
        Assert.Equal("Mueva dinero de una cuenta a otra", modelo.TransferenciaDetalle);
    }

    [Fact]
    public void Cliente_no_puede_depositar_y_ve_los_textos_de_cliente()
    {
        var modelo = new MenuViewModel(Sesiones.Cliente(), _ => { });

        Assert.False(modelo.PuedeDepositar);
        Assert.Equal("Próximamente", modelo.DepositoDetalle);
        Assert.Equal("Registrar una salida de su cuenta", modelo.RetiroDetalle);
        Assert.Equal("Mover fondos desde su cuenta", modelo.TransferenciaDetalle);
    }

    [Theory]
    [InlineData("deposito", ModoOperacion.Deposito)]
    [InlineData("retiro", ModoOperacion.Retiro)]
    [InlineData("transferencia", ModoOperacion.Transferencia)]
    [InlineData("movimientos", ModoOperacion.Movimientos)]
    public void Cada_tarjeta_abre_su_pantalla(string parametro, ModoOperacion esperado)
    {
        ModoOperacion abierto = ModoOperacion.Deposito;
        var modelo = new MenuViewModel(Sesiones.Empleado(), modo => abierto = modo);

        modelo.AbrirCommand.Execute(parametro);

        Assert.Equal(esperado, abierto);
    }
}
