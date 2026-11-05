using ec.edu.monster.config;
using ec.edu.monster.controlador;
using ec.edu.monster.fakes;
using ec.edu.monster.modelo;
using ec.edu.monster.modelo.servicios;

namespace ec.edu.monster.vista.modelos;

/// <summary>
/// State of the "Consultar movimientos" screen: the account a Cliente cannot type, the result card,
/// the empty history, the server error and the back button.
/// </summary>
public class MovimientosViewModelTest
{
    private readonly FakeTransporte _transporte = new();

    private MovimientosViewModel Crear(Sesion sesion, bool esCliente = false, string cuenta = "",
        Action? alVolver = null)
        => new(new MovimientosControlador(sesion,
                new MovimientoServicio(_transporte, new Configuracion("10.0.0.5", 5171))),
            esCliente, cuenta, alVolver ?? (() => { }));

    private static void ResponderHistorial(FakeTransporte transporte, bool conMovimientos = true)
        => transporte.Responder("consultarMovimientos", conMovimientos
            ? SoapRespuestas.ConsultaOk((6, "2026-11-05T10:00:00", "DEPOSITO", "INGRESO", "100.50", "00100001"))
            : SoapRespuestas.ConsultaOk());

    [Fact]
    public void Cliente_arranca_con_su_cuenta_y_sin_busqueda()
    {
        MovimientosViewModel modelo = Crear(Sesiones.Cliente("00100001"), esCliente: true, cuenta: "00100001");

        Assert.True(modelo.EsCliente);
        Assert.False(modelo.MuestraBusqueda);
        Assert.Equal("00100001", modelo.Cuenta);
        Assert.False(modelo.HayHistorial);
    }

    [Fact]
    public async Task Cliente_carga_su_historial_al_abrir_la_pantalla()
    {
        ResponderHistorial(_transporte);
        MovimientosViewModel modelo = Crear(Sesiones.Cliente("00100001"), esCliente: true, cuenta: "00100001");

        await modelo.ConsultarAsync();

        Assert.True(modelo.HayHistorial);
        Assert.False(modelo.SinMovimientos);
        Assert.Equal("00100001", modelo.Numero);
        Assert.Equal("Ana Pérez", modelo.Titular);
        Assert.Equal("$ 379.50", modelo.Saldo);
        Assert.Single(modelo.Movimientos);
        Assert.False(modelo.HayErrorGeneral);
    }

    [Fact]
    public async Task Empleado_sin_cuenta_muestra_el_error_sin_llamar_al_servidor()
    {
        MovimientosViewModel modelo = Crear(Sesiones.Empleado());

        await modelo.ConsultarAsync();

        Assert.True(modelo.HayErrorGeneral);
        Assert.Equal(MovimientosControlador.CuentaVacia, modelo.ErrorGeneral);
        Assert.False(modelo.HayHistorial);
        Assert.Equal(0, _transporte.Llamadas("consultarMovimientos"));
    }

    [Fact]
    public async Task Error_del_servidor_oculta_el_resultado()
    {
        _transporte.Responder("consultarMovimientos",
            SoapRespuestas.Error("consultarMovimientos", "La cuenta no existe"));
        MovimientosViewModel modelo = Crear(Sesiones.Empleado());
        modelo.Cuenta = "00100001";

        await modelo.ConsultarAsync();

        Assert.False(modelo.HayHistorial);
        Assert.Equal("La cuenta no existe", modelo.ErrorGeneral);
    }

    [Fact]
    public async Task Cuenta_sin_movimientos_lo_indica()
    {
        ResponderHistorial(_transporte, conMovimientos: false);
        MovimientosViewModel modelo = Crear(Sesiones.Empleado());
        modelo.Cuenta = "00100001";

        await modelo.ConsultarAsync();

        Assert.True(modelo.HayHistorial);
        Assert.True(modelo.SinMovimientos);
        Assert.False(modelo.HayMovimientos);
    }

    [Fact]
    public void Volver_llama_al_callback()
    {
        bool volvio = false;
        MovimientosViewModel modelo = Crear(Sesiones.Empleado(), alVolver: () => volvio = true);

        modelo.VolverCommand.Execute(null);

        Assert.True(volvio);
    }
}
