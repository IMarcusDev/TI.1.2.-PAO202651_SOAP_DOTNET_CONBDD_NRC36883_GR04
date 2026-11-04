using MOV_EUREKABANK_DOTNET_SOAP.Controlador;
using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Data;
using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Enums;
using MOV_EUREKABANK_DOTNET_SOAP.Utils;
using MOV_EUREKABANK_DOTNET_SOAP.Vista.Componentes;

namespace MOV_EUREKABANK_DOTNET_SOAP.Vista;

/// <summary>Inicio: saludo, cuenta del cliente (o ventanilla del empleado) y accesos a las opciones del perfil.</summary>
public partial class InicioPage : PestanaPage
{
    private const int UltimosMovimientos = 3;

    private readonly MovimientosControlador _controlador;
    private readonly OperacionControlador _operaciones;
    private bool _cargando;
    private bool _accionesCreadas;

    public InicioPage(MovimientosControlador controlador, OperacionControlador operaciones, LoginControlador login)
        : base(login, Textos.AppNombre)
    {
        InitializeComponent();
        _controlador = controlador;
        _operaciones = operaciones;

        Refrescar.Command = new Command(async () => await CargarCuentaAsync());
        BtnReintentarCuenta.Clicked += async (_, _) => await CargarCuentaAsync();
        BtnVerTodos.Clicked += async (_, _) => await Shell.Current.GoToAsync(NavegacionSesion.RutaMovimientos);
    }

    protected override ControladorConSesion Controlador => _controlador;

    protected override void AlMostrar()
    {
        Sesion sesion = _controlador.Sesion!;
        LblNombre.Text = string.IsNullOrEmpty(sesion.Nombre) ? Formato.SinDato : sesion.Nombre;
        LblPerfil.Text = string.Format(Textos.InicioPerfil, sesion.Perfil.Etiqueta());

        bool empleado = sesion.EsEmpleado;
        TarjetaVentanilla.IsVisible = empleado;
        TarjetaCuenta.IsVisible = !empleado;
        SeccionUltimos.IsVisible = !empleado;
        Refrescar.IsRefreshEnabled = !empleado;

        if (!_accionesCreadas)
        {
            CrearAcciones(empleado);
            _accionesCreadas = true;
        }
        if (!empleado)
            _ = CargarCuentaAsync();
    }

    private void CrearAcciones(bool empleado)
    {
        var acciones = new List<View>
        {
            Filas.Accion(Iconos.Lista, Textos.AccionConsultar,
                empleado ? Textos.AccionConsultarDetalleEmpleado : Textos.AccionConsultarDetalleCliente,
                async () => await Shell.Current.GoToAsync(NavegacionSesion.RutaMovimientos)),
        };
        foreach (TipoOperacion tipo in _operaciones.TiposDisponibles())
        {
            acciones.Add(Filas.Accion(Textos.IconoOperacion(tipo), Textos.NombreOperacion(tipo), Textos.DescripcionOperacion(tipo),
                async () => await Shell.Current.GoToAsync($"{NavegacionSesion.RutaOperaciones}?tipo={tipo}")));
        }
        Filas.Llenar(ListaAcciones, acciones);
    }

    private async Task CargarCuentaAsync()
    {
        if (_cargando || _controlador.EsEmpleado)
        {
            Refrescar.IsRefreshing = false;
            return;
        }
        _cargando = true;
        if (string.IsNullOrEmpty(LblNumeroCuenta.Text))
            MostrarEstadoCarga(Textos.InicioCargandoCuenta, false);

        Resultado<ConsultaMovimientos> resultado;
        try
        {
            resultado = await _controlador.ConsultarAsync(null);
        }
        catch (Exception)
        {
            resultado = Resultado<ConsultaMovimientos>.Error(Textos.ErrorInesperado);
        }
        _cargando = false;
        Refrescar.IsRefreshing = false;

        if (resultado.SesionExpirada)
        {
            await VolverAlLoginAsync(resultado.Mensaje);
            return;
        }
        if (!resultado.Exito)
        {
            if (string.IsNullOrEmpty(LblNumeroCuenta.Text))
                MostrarEstadoCarga(resultado.Mensaje, true);
            return;
        }

        ConsultaMovimientos consulta = resultado.Datos!;
        MostrarCuenta(consulta.Cuenta);
        MostrarUltimos(consulta.MovimientosRecientes, consulta.Cuenta.Moneda);
    }

    private void MostrarEstadoCarga(string mensaje, bool conError)
    {
        DatosCuenta.Opacity = 0;
        EstadoCargaCuenta.IsVisible = true;
        PrgCuenta.IsVisible = PrgCuenta.IsRunning = !conError;
        BtnReintentarCuenta.IsVisible = conError;
        LblEstadoCarga.Text = mensaje;
    }

    private void MostrarCuenta(Cuenta cuenta)
    {
        EstadoCargaCuenta.IsVisible = false;
        DatosCuenta.Opacity = 1;
        LblNumeroCuenta.Text = string.Format(Textos.InicioNumeroCuenta, cuenta.Numero);
        LblSaldo.Text = Formato.Importe(cuenta.Saldo, cuenta.Moneda);
        LblTitular.Text = cuenta.Titular;
        LblEstadoCuenta.Text = cuenta.EstadoVisible;
    }

    /// <summary>Muestra los últimos movimientos; la lista ya viene del más reciente al más antiguo.</summary>
    private void MostrarUltimos(IReadOnlyList<Movimiento> recientes, string moneda)
    {
        Filas.Llenar(ListaUltimos, recientes
            .Take(UltimosMovimientos)
            .Select(m => Filas.Movimiento(m, moneda)));
        LblSinMovimientos.IsVisible = recientes.Count == 0;
    }
}
