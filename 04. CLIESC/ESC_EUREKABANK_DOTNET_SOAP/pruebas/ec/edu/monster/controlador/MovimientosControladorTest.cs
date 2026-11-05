using ec.edu.monster.config;
using ec.edu.monster.fakes;
using ec.edu.monster.modelo;
using ec.edu.monster.modelo.dto;
using ec.edu.monster.modelo.servicios;

namespace ec.edu.monster.controlador;

/// <summary>
/// "Consultar movimientos" flow: empty account, inactive session, the request the server receives
/// and the error it answers.
/// </summary>
public class MovimientosControladorTest
{
    private readonly FakeTransporte _transporte = new();

    private MovimientosControlador Crear(Sesion sesion)
        => new(sesion, new MovimientoServicio(_transporte, new Configuracion("10.0.0.5", 5171)));

    [Fact]
    public async Task Cuenta_vacia_no_llama_al_servidor()
    {
        HistorialCuenta historial = await Crear(Sesiones.Empleado()).ConsultarAsync("  ");

        Assert.False(historial.Ok);
        Assert.Equal(MovimientosControlador.CuentaVacia, historial.Mensaje);
        Assert.Equal(0, _transporte.Llamadas("consultarMovimientos"));
    }

    [Fact]
    public async Task Sesion_inactiva_no_llama_al_servidor()
    {
        HistorialCuenta historial = await Crear(Sesiones.Inactiva()).ConsultarAsync("00100001");

        Assert.False(historial.Ok);
        Assert.Equal(MovimientosControlador.SesionInactiva, historial.Mensaje);
        Assert.Equal(0, _transporte.Llamadas("consultarMovimientos"));
    }

    [Fact]
    public async Task Consulta_envia_token_y_cuenta_sin_espacios()
    {
        _transporte.Responder("consultarMovimientos", SoapRespuestas.ConsultaOk(
            (6, "2026-11-05T10:00:00", "DEPOSITO", "INGRESO", "100.50", "00100001")));

        HistorialCuenta historial = await Crear(Sesiones.Empleado()).ConsultarAsync(" 00100001 ");

        PeticionSoap peticion = _transporte.Ultima("consultarMovimientos");
        Assert.Equal("token-123", peticion.Parametro("token"));
        Assert.Equal("00100001", peticion.Parametro("cuenta"));
        Assert.True(historial.Ok);
        Assert.Equal("Ana Pérez", historial.Cuenta!.Titular);
        Assert.Single(historial.Movimientos);
    }

    [Fact]
    public async Task Error_del_servidor_devuelve_su_mensaje()
    {
        _transporte.Responder("consultarMovimientos",
            SoapRespuestas.Error("consultarMovimientos", "La cuenta no existe"));

        HistorialCuenta historial = await Crear(Sesiones.Empleado()).ConsultarAsync("00100001");

        Assert.False(historial.Ok);
        Assert.Equal("La cuenta no existe", historial.Mensaje);
        Assert.Null(historial.Cuenta);
    }
}
