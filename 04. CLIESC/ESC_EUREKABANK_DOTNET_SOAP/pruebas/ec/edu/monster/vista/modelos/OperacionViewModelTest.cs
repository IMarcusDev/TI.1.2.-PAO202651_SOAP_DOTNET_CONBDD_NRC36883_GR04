using ec.edu.monster.controlador;
using ec.edu.monster.modelo;
using ec.edu.monster.modelo.dto;
using ec.edu.monster.fakes;
using ec.edu.monster.vista.modelos;

namespace ec.edu.monster.vista.modelos;

/// <summary>
/// Movement form state: which fields each role sees, the form built from them, the per-field
/// errors and the receipt callback.
/// </summary>
public class OperacionViewModelTest
{
    private static (OperacionViewModel Modelo, List<MovimientoForm> Llamadas, List<Comprobante> Recibos) Crear(
        ModoOperacion modo, bool esCliente, string cuentaCliente = "00100001",
        MovimientoResultado? respuesta = null, bool confirmar = true)
    {
        var llamadas = new List<MovimientoForm>();
        var recibos = new List<Comprobante>();
        MovimientoResultado exito = respuesta ?? new MovimientoResultado(true, "Registrado", "7", 250m);
        var controlador = new MovimientoControlador(Sesiones.Empleado(),
            _ => "¿Seguro?",
            (_, form) =>
            {
                llamadas.Add(form);
                return Task.FromResult(exito);
            },
            Comprobante.Deposito);

        var modelo = new OperacionViewModel(controlador, modo, esCliente, cuentaCliente,
            recibos.Add, _ => confirmar, () => { });
        return (modelo, llamadas, recibos);
    }

    [Fact]
    public async Task Deposito_arma_el_formulario_y_muestra_el_comprobante()
    {
        var (modelo, llamadas, recibos) = Crear(ModoOperacion.Deposito, esCliente: false);
        modelo.Cuenta = "00100001";
        modelo.Importe = "100,50";

        await modelo.RevisarAsync();

        MovimientoForm form = Assert.Single(llamadas);
        Assert.Equal("00100001", form.Cuenta);
        Assert.Equal(100.50m, form.Importe!.Value);
        Assert.Single(recibos);
        Assert.Equal("", modelo.Importe);
    }

    [Fact]
    public async Task Cliente_retira_de_su_propia_cuenta_sin_clave()
    {
        var (modelo, llamadas, _) = Crear(ModoOperacion.Retiro, esCliente: true, "00100001");
        modelo.Importe = "50";

        await modelo.RevisarAsync();

        MovimientoForm form = Assert.Single(llamadas);
        Assert.Equal("00100001", form.Cuenta);
        Assert.Equal("", form.Clave);
        Assert.True(modelo.MuestraCuentaFija);
        Assert.False(modelo.MuestraClave);
    }

    [Fact]
    public async Task Empleado_sin_clave_ve_el_error_y_no_llama_al_servidor()
    {
        var (modelo, llamadas, recibos) = Crear(ModoOperacion.Retiro, esCliente: false);
        modelo.Cuenta = "00100001";
        modelo.Importe = "10";

        await modelo.RevisarAsync();

        Assert.Equal(MovimientoForm.ClaveVacia, modelo.Errores["clave"]);
        Assert.Empty(llamadas);
        Assert.Empty(recibos);
        Assert.True(modelo.MuestraClave);
    }

    [Fact]
    public async Task Cliente_transfiere_desde_su_cuenta_hacia_el_destino()
    {
        var (modelo, llamadas, _) = Crear(ModoOperacion.Transferencia, esCliente: true, "00100001");
        modelo.Destino = "00200002";
        modelo.Importe = "75";

        await modelo.RevisarAsync();

        MovimientoForm form = Assert.Single(llamadas);
        Assert.Equal("00100001", form.Origen);
        Assert.Equal("00200002", form.Destino);
        Assert.Equal("", form.Clave);
    }

    [Fact]
    public async Task Importe_vacio_se_informa_por_campo()
    {
        var (modelo, llamadas, _) = Crear(ModoOperacion.Deposito, esCliente: false);
        modelo.Cuenta = "00100001";

        await modelo.RevisarAsync();

        Assert.Equal(MovimientoForm.ImporteVacio, modelo.Errores["importe"]);
        Assert.Empty(llamadas);
    }

    [Fact]
    public async Task Cancelar_no_llama_al_servidor_ni_muestra_error()
    {
        var (modelo, llamadas, recibos) = Crear(ModoOperacion.Deposito, esCliente: false, confirmar: false);
        modelo.Cuenta = "00100001";
        modelo.Importe = "10";

        await modelo.RevisarAsync();

        Assert.Empty(llamadas);
        Assert.Empty(recibos);
        Assert.False(modelo.HayErrorGeneral);
    }

    [Fact]
    public async Task Error_del_servidor_se_muestra_debajo_del_formulario()
    {
        var (modelo, _, recibos) = Crear(ModoOperacion.Deposito, esCliente: false,
            respuesta: MovimientoResultado.Error("Saldo insuficiente"));
        modelo.Cuenta = "00100001";
        modelo.Importe = "10";

        await modelo.RevisarAsync();

        Assert.True(modelo.HayErrorGeneral);
        Assert.Equal("Saldo insuficiente", modelo.ErrorGeneral);
        Assert.Empty(recibos);
    }

    [Fact]
    public void Limpiar_borra_los_campos_y_los_errores()
    {
        var (modelo, _, _) = Crear(ModoOperacion.Transferencia, esCliente: false);
        modelo.Origen = "00100001";
        modelo.Destino = "00200002";
        modelo.Importe = "10";

        modelo.Limpiar();

        Assert.Equal("", modelo.Origen);
        Assert.Equal("", modelo.Destino);
        Assert.Equal("", modelo.Importe);
        Assert.All(modelo.Errores.Values, valor => Assert.Equal("", valor));
    }

    [Fact]
    public void Titulos_y_etiquetas_cambian_con_el_modo()
    {
        var (deposito, _, _) = Crear(ModoOperacion.Deposito, esCliente: false);
        var (retiro, _, _) = Crear(ModoOperacion.Retiro, esCliente: false);
        var (transferencia, _, _) = Crear(ModoOperacion.Transferencia, esCliente: false);

        Assert.Equal("Depósito", deposito.Titulo);
        Assert.Equal("Cuenta de destino", deposito.EtiquetaCuenta);
        Assert.Equal("Cuenta de origen", retiro.EtiquetaCuenta);
        Assert.Equal("Clave de la cuenta", retiro.EtiquetaClave);
        Assert.Equal("Transferencia", transferencia.Titulo);
        Assert.Equal("Clave de la cuenta de origen", transferencia.EtiquetaClave);
    }
}
