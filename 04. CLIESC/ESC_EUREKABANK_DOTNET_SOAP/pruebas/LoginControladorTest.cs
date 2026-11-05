using ec.edu.monster.config;
using ec.edu.monster.controlador;
using ec.edu.monster.modelo;
using ec.edu.monster.modelo.servicios;
using ec.edu.monster.pruebas.fakes;

namespace ec.edu.monster.pruebas;

/// <summary>
/// Login rules of the desktop client: which fields are required per role, that the account is the
/// identifier for a Cliente, and that a failed login never starts the session.
/// </summary>
public class LoginControladorTest
{
    private static (LoginControlador Controlador, FakeTransporte Transporte, Sesion Sesion) Crear()
    {
        var transporte = new FakeTransporte();
        var servicio = new LoginServicio(transporte, new Configuracion());
        var sesion = new Sesion();
        return (new LoginControlador(servicio, sesion), transporte, sesion);
    }

    [Fact]
    public async Task Empleado_sin_datos_pide_usuario_y_contrasena()
    {
        var (controlador, transporte, sesion) = Crear();

        string? error = await controlador.IngresarAsync("   ", "", esCliente: false);

        Assert.Equal(LoginControlador.CredencialesVaciasEmpleado, error);
        Assert.Empty(transporte.Peticiones);
        Assert.False(sesion.EstaActiva);
    }

    [Fact]
    public async Task Cliente_sin_datos_pide_cuenta_y_clave()
    {
        var (controlador, transporte, sesion) = Crear();

        string? error = await controlador.IngresarAsync("", "  ", esCliente: true);

        Assert.Equal(LoginControlador.CredencialesVaciasCliente, error);
        Assert.Empty(transporte.Peticiones);
        Assert.False(sesion.EstaActiva);
    }

    [Fact]
    public async Task Empleado_ingresa_con_usuario_recortado_y_clave_intacta()
    {
        var (controlador, transporte, sesion) = Crear();
        transporte.Responder("loginEmpleado",
            SoapRespuestas.LoginOk("loginEmpleado", "Ana Pérez", "Empleado"));

        string? error = await controlador.IngresarAsync("  EMP01  ", " clave ", esCliente: false);

        Assert.Null(error);
        Assert.True(sesion.EsEmpleado);
        Assert.Equal("EMP01", sesion.Usuario);
        Assert.Equal("Ana Pérez", sesion.Nombre);
        Assert.Equal("token-123", sesion.Token);

        PeticionSoap peticion = transporte.Ultima("loginEmpleado");
        Assert.Equal("EMP01", peticion.Parametro("usuario"));
        Assert.Equal(" clave ", peticion.Parametro("clave"));
    }

    [Fact]
    public async Task Cliente_ingresa_con_la_cuenta_como_identificador()
    {
        var (controlador, transporte, sesion) = Crear();
        transporte.Responder("loginCliente",
            SoapRespuestas.LoginOk("loginCliente", "Luis Torres", "Cliente", "token-cli"));

        string? error = await controlador.IngresarAsync("00100001", "clave", esCliente: true);

        Assert.Null(error);
        Assert.True(sesion.EsCliente);
        Assert.Equal("00100001", sesion.Cuenta);
        Assert.Equal("00100001", transporte.Ultima("loginCliente").Parametro("cuenta"));
        Assert.Equal(0, transporte.Llamadas("loginEmpleado"));
    }

    [Fact]
    public async Task Credenciales_incorrectas_devuelven_el_mensaje_y_no_inician_sesion()
    {
        var (controlador, transporte, sesion) = Crear();
        transporte.Responder("loginEmpleado",
            SoapRespuestas.Error("loginEmpleado", "Credenciales incorrectas"));

        string? error = await controlador.IngresarAsync("EMP01", "mala", esCliente: false);

        Assert.Equal("Credenciales incorrectas", error);
        Assert.False(sesion.EstaActiva);
    }

    [Fact]
    public async Task Cerrar_sesion_envia_el_token_y_limpia_el_estado_local()
    {
        var (controlador, transporte, sesion) = Crear();
        transporte.Responder("loginEmpleado",
            SoapRespuestas.LoginOk("loginEmpleado", "Ana Pérez", "Empleado"));
        transporte.Responder("logout", SoapRespuestas.Sobre("logout", "<Estado>OK</Estado>"));
        await controlador.IngresarAsync("EMP01", "clave", esCliente: false);

        await controlador.CerrarSesionAsync();

        Assert.False(sesion.EstaActiva);
        Assert.Equal("token-123", transporte.Ultima("logout").Parametro("token"));
    }
}
