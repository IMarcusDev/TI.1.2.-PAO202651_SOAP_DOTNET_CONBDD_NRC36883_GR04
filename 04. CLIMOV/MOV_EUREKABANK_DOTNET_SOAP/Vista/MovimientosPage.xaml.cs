using MOV_EUREKABANK_DOTNET_SOAP.Controlador;
using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Data;
using MOV_EUREKABANK_DOTNET_SOAP.Utils;
using MOV_EUREKABANK_DOTNET_SOAP.Vista.Componentes;

namespace MOV_EUREKABANK_DOTNET_SOAP.Vista;

/// <summary>ECUD2 Consultar Movimientos: cabecera con los datos de la cuenta y lista de movimientos.</summary>
public partial class MovimientosPage : PestanaPage
{
    private readonly MovimientosControlador _controlador;
    private bool _cargando;
    /// <summary>Última cuenta consultada con éxito por el empleado, para recargarla al deslizar.</summary>
    private string? _cuentaConsultada;

    public MovimientosPage(MovimientosControlador controlador, LoginControlador login)
        : base(login, Textos.PestanaMovimientos)
    {
        InitializeComponent();
        _controlador = controlador;

        Refrescar.Command = new Command(async () =>
        {
            if (_controlador.EsEmpleado && _cuentaConsultada is null)
                Refrescar.IsRefreshing = false;
            else
                await ConsultarAsync(_cuentaConsultada);
        });

        BtnConsultar.Clicked += async (_, _) => await ConsultarDesdeFormularioAsync();
        CampoCuenta.Completado += async (_, _) => await ConsultarDesdeFormularioAsync();
    }

    protected override ControladorConSesion Controlador => _controlador;

    protected override void AlMostrar()
    {
        bool empleado = _controlador.EsEmpleado;
        TarjetaBusqueda.IsVisible = empleado;

        if (empleado)
        {
            if (_cuentaConsultada is null && !_cargando && !PanelResultado.IsVisible)
                PanelInicial.IsVisible = true;
        }
        else
        {
            // El cliente siempre ve su propia cuenta; se recarga para reflejar sus últimas operaciones,
            // en segundo plano si ya hay datos en pantalla.
            if (!_cargando && PanelResultado.IsVisible)
                Refrescar.IsRefreshing = true;
            else
                _ = ConsultarAsync(null);
        }
    }

    private async Task ConsultarDesdeFormularioAsync()
    {
        string cuenta = CampoCuenta.Texto;
        ValidacionResultado validacion = _controlador.Validar(cuenta);
        if (!validacion.Valido)
        {
            CampoCuenta.MostrarError(validacion.Mensaje);
            return;
        }
        CampoCuenta.QuitarFoco();
        await ConsultarAsync(cuenta.Trim());
    }

    private async Task ConsultarAsync(string? cuenta)
    {
        if (_cargando)
            return;
        _cargando = true;
        MostrarCargando(true);

        Resultado<ConsultaMovimientos> resultado;
        try
        {
            resultado = await _controlador.ConsultarAsync(cuenta);
        }
        catch (Exception)
        {
            resultado = Resultado<ConsultaMovimientos>.Error(Textos.ErrorInesperado);
        }
        _cargando = false;
        MostrarCargando(false);

        if (resultado.SesionExpirada)
        {
            await VolverAlLoginAsync(resultado.Mensaje);
            return;
        }
        if (!resultado.Exito)
        {
            // ECUD2 FA3: se muestra el mensaje y se conserva el número ingresado para corregirlo.
            PanelResultado.IsVisible = false;
            LblError.Texto = resultado.Mensaje;
            return;
        }

        _cuentaConsultada = cuenta;
        MostrarResultado(resultado.Datos!);
    }

    private void MostrarResultado(ConsultaMovimientos consulta)
    {
        Cuenta cuenta = consulta.Cuenta;
        LblError.Texto = null;
        PanelInicial.IsVisible = false;
        PanelResultado.IsVisible = true;

        LblNumero.Text = cuenta.Numero;
        bool activa = cuenta.EstaActiva;
        LblEstado.Text = cuenta.EstadoVisible;
        LblEstado.TextColor = activa ? Colores.Ingreso : Colores.Salida;
        PastillaEstado.BackgroundColor = activa ? Colores.IngresoFondo : Colores.SalidaFondo;
        LblTitular.Text = string.IsNullOrEmpty(cuenta.Titular) ? Formato.SinDato : cuenta.Titular;
        LblSaldo.Text = Formato.Importe(cuenta.Saldo);
        LblMoneda.Text = string.IsNullOrEmpty(cuenta.Moneda) ? Formato.SinDato : cuenta.Moneda;

        int cantidad = consulta.Movimientos.Count;
        LblCantidad.Text = Textos.MovCantidad(cantidad);
        Filas.Llenar(ListaMovimientos, consulta.MovimientosRecientes.Select(m => Filas.Movimiento(m, cuenta.Moneda)));
        ListaMovimientos.IsVisible = cantidad > 0;
        LblVacio.IsVisible = cantidad == 0;
    }

    private void MostrarCargando(bool activo)
    {
        if (!Refrescar.IsRefreshing)
        {
            PanelCargando.IsVisible = activo;
            if (activo)
            {
                PanelInicial.IsVisible = false;
                PanelResultado.IsVisible = false;
                LblError.Texto = null;
            }
        }
        if (!activo)
            Refrescar.IsRefreshing = false;
        BtnConsultar.IsEnabled = !activo;
        CampoCuenta.IsEnabled = !activo;
    }
}
