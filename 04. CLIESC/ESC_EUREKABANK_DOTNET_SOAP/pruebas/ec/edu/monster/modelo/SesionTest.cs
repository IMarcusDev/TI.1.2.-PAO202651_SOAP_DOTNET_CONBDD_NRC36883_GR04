using ec.edu.monster.modelo;
using ec.edu.monster.modelo.dto;

namespace ec.edu.monster.modelo;

public class SesionTest
{
    [Fact]
    public void AClienteWorksOnTheAccountUsedToLogIn()
    {
        var sesion = new Sesion();
        sesion.Iniciar("00100001", new LoginResultado(true, "ok", "token-1", "Ana Pérez", Sesion.RolCliente));

        Assert.True(sesion.EstaActiva);
        Assert.True(sesion.EsCliente);
        Assert.False(sesion.EsEmpleado);
        Assert.Equal("00100001", sesion.Cuenta);
        Assert.Equal("Ana Pérez", sesion.NombreVisible);
    }

    [Fact]
    public void AnEmpleadoHasNoOwnAccount()
    {
        var sesion = new Sesion();
        sesion.Iniciar("monster", new LoginResultado(true, "ok", "token-1", "Monster Bank", Sesion.RolEmpleado));

        Assert.True(sesion.EsEmpleado);
        Assert.False(sesion.EsCliente);
        Assert.Equal("", sesion.Cuenta);
    }

    [Fact]
    public void AnEmptyNameFallsBackToTheUser()
    {
        var sesion = new Sesion();
        sesion.Iniciar("monster", new LoginResultado(true, "ok", "token-1", " ", Sesion.RolEmpleado));

        Assert.Equal("monster", sesion.NombreVisible);
    }

    [Fact]
    public void CerrarClearsEverything()
    {
        var sesion = new Sesion();
        sesion.Iniciar("00100001", new LoginResultado(true, "ok", "token-1", "Ana", Sesion.RolCliente));

        sesion.Cerrar();

        Assert.False(sesion.EstaActiva);
        Assert.False(sesion.EsCliente);
        Assert.Equal("", sesion.Usuario);
        Assert.Equal("", sesion.Token);
    }
}
