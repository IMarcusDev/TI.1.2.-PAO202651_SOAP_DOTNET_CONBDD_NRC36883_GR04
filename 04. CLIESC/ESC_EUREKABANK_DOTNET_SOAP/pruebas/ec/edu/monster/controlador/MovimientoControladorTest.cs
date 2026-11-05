using ec.edu.monster.controlador;
using ec.edu.monster.modelo;
using ec.edu.monster.modelo.dto;
using ec.edu.monster.fakes;

namespace ec.edu.monster.controlador;

/// <summary>
/// Flow of one movement screen: invalid form, declined confirmation, inactive session, server
/// error and the receipt built from the answer.
/// </summary>
public class MovimientoControladorTest
{
    private static MovimientoControlador Crear(Sesion sesion, List<MovimientoForm> llamadas,
        MovimientoResultado respuesta)
        => new(sesion,
            form => $"¿Depositar {form.ImporteFormateado} en la cuenta {form.Cuenta}?",
            (_, form) =>
            {
                llamadas.Add(form);
                return Task.FromResult(respuesta);
            },
            Comprobante.Deposito);

    private static MovimientoResultado Exito()
        => new(true, "Depósito registrado", "6", 379.50m);

    [Fact]
    public async Task Formulario_invalido_no_pregunta_ni_llama_al_servidor()
    {
        var llamadas = new List<MovimientoForm>();
        MovimientoControlador controlador = Crear(Sesiones.Empleado(), llamadas, Exito());
        int preguntas = 0;

        ResultadoRevision resultado = await controlador.RevisarAsync(
            MovimientoForm.Deposito("", ""), _ => { preguntas++; return true; });

        Assert.Equal(EstadoRevision.Invalido, resultado.Estado);
        Assert.Equal(0, preguntas);
        Assert.Empty(llamadas);
    }

    [Fact]
    public async Task Cancelar_la_confirmacion_no_llama_al_servidor()
    {
        var llamadas = new List<MovimientoForm>();
        MovimientoControlador controlador = Crear(Sesiones.Empleado(), llamadas, Exito());

        ResultadoRevision resultado = await controlador.RevisarAsync(
            MovimientoForm.Deposito("00100001", "100"), _ => false);

        Assert.Equal(EstadoRevision.Cancelado, resultado.Estado);
        Assert.Empty(llamadas);
    }

    [Fact]
    public async Task Sesion_inactiva_no_llama_al_servidor()
    {
        var llamadas = new List<MovimientoForm>();
        MovimientoControlador controlador = Crear(Sesiones.Inactiva(), llamadas, Exito());

        ResultadoRevision resultado = await controlador.RevisarAsync(
            MovimientoForm.Deposito("00100001", "100"), _ => true);

        Assert.Equal(EstadoRevision.Error, resultado.Estado);
        Assert.Equal(MovimientoControlador.SesionInactiva, resultado.Mensaje);
        Assert.Empty(llamadas);
    }

    [Fact]
    public async Task Error_del_servidor_devuelve_su_mensaje()
    {
        var llamadas = new List<MovimientoForm>();
        MovimientoControlador controlador = Crear(Sesiones.Empleado(), llamadas,
            MovimientoResultado.Error("Saldo insuficiente"));

        ResultadoRevision resultado = await controlador.RevisarAsync(
            MovimientoForm.Deposito("00100001", "100"), _ => true);

        Assert.Equal(EstadoRevision.Error, resultado.Estado);
        Assert.Equal("Saldo insuficiente", resultado.Mensaje);
        Assert.Null(resultado.Comprobante);
        Assert.Single(llamadas);
    }

    [Fact]
    public async Task Exito_devuelve_el_comprobante_armado()
    {
        var llamadas = new List<MovimientoForm>();
        MovimientoControlador controlador = Crear(Sesiones.Empleado(), llamadas, Exito());

        ResultadoRevision resultado = await controlador.RevisarAsync(
            MovimientoForm.Deposito("00100001", "100.50"), _ => true);

        Assert.Equal(EstadoRevision.Exito, resultado.Estado);
        Comprobante comprobante = Assert.IsType<Comprobante>(resultado.Comprobante);
        Assert.Equal("Depósito", comprobante.Operacion);
        Assert.Equal("00100001", comprobante.CuentaDestino);
        Assert.Equal("$ 100.50", comprobante.Importe);
        Assert.Equal("000006", comprobante.NumeroMovimiento);
        Assert.Equal("$ 379.50", comprobante.NuevoSaldo);
        Assert.Equal("Depósito registrado", comprobante.Mensaje);
    }

    [Fact]
    public void La_pregunta_incluye_importe_y_cuenta()
    {
        MovimientoControlador controlador = Crear(Sesiones.Empleado(), [], Exito());

        string pregunta = controlador.Pregunta(MovimientoForm.Deposito("00100001", "100.50"));

        Assert.Equal("¿Depositar $ 100.50 en la cuenta 00100001?", pregunta);
    }
}
