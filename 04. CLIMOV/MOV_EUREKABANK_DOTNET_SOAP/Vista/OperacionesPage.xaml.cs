using MOV_EUREKABANK_DOTNET_SOAP.Controlador;
using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Data;
using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Enums;
using MOV_EUREKABANK_DOTNET_SOAP.Utils;
using MOV_EUREKABANK_DOTNET_SOAP.Vista.Componentes;

namespace MOV_EUREKABANK_DOTNET_SOAP.Vista;

/// <summary>
/// ECUD3 Registrar Movimiento: formulario del tipo elegido según el perfil, resumen para confirmar
/// y comprobante con el número de movimiento y el nuevo saldo.
/// </summary>
public partial class OperacionesPage : PestanaPage, IQueryAttributable
{
    private readonly OperacionControlador _controlador;
    private TipoOperacion? _tipo;
    private TipoOperacion? _tipoSolicitado;
    private bool _configurado;
    private bool _registrando;
    private bool _cargandoCuenta;

    public OperacionesPage(OperacionControlador controlador, LoginControlador login)
        : base(login, Textos.PestanaOperaciones)
    {
        InitializeComponent();
        _controlador = controlador;

        GrupoTipo.SeleccionCambiada += (_, tipo) => AplicarTipo((TipoOperacion)tipo);
        CampoCuenta.Completado += (_, _) => (CampoDestino.IsVisible ? CampoDestino : CampoImporte).Enfocar();
        CampoDestino.Completado += (_, _) => CampoImporte.Enfocar();
        CampoImporte.Completado += async (_, _) =>
        {
            if (CampoClave.IsVisible)
                CampoClave.Enfocar();
            else
                await AlRegistrarAsync();
        };
        CampoClave.Completado += async (_, _) => await AlRegistrarAsync();
        BtnRegistrar.Pulsado += async (_, _) => await AlRegistrarAsync();
    }

    protected override ControladorConSesion Controlador => _controlador;

    /// <summary>Acciones rápidas del inicio: abren esta pestaña con el tipo ya elegido (?tipo=Retiro).</summary>
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("tipo", out object? valor) && Enum.TryParse(valor?.ToString(), out TipoOperacion tipo))
        {
            _tipoSolicitado = tipo;
            if (_configurado)
                TomarTipoSolicitado();
        }
    }

    protected override void AlMostrar()
    {
        if (!_configurado)
        {
            ConfigurarPerfil();
            _configurado = true;
        }
        TomarTipoSolicitado();

        if (!_controlador.PideCuenta)
        {
            MostrarCuentaOrigen(_controlador.CuentaClienteGuardada);
            _ = CargarCuentaClienteAsync();
        }
    }

    private void TomarTipoSolicitado()
    {
        TipoOperacion? pedido = _tipoSolicitado;
        _tipoSolicitado = null;
        if (pedido is TipoOperacion tipo && _controlador.TiposDisponibles().Contains(tipo))
            Seleccionar(tipo);
    }

    /// <summary>ECUD3 paso 1: al cliente solo se le muestran Retiro y Transferencia.</summary>
    private void ConfigurarPerfil()
    {
        IReadOnlyList<TipoOperacion> tipos = _controlador.TiposDisponibles();
        GrupoTipo.Configurar(tipos.Select(t => new Segmentos.Opcion(t, Textos.NombreOperacion(t), Textos.IconoOperacion(t))));
        TarjetaOrigen.IsVisible = !_controlador.PideCuenta;
        Seleccionar(tipos[0]);
    }

    private void Seleccionar(TipoOperacion nuevo)
    {
        GrupoTipo.Seleccion = nuevo;
        AplicarTipo(nuevo);
    }

    /// <summary>ECUD3 paso 2: muestra los campos del tipo elegido según el perfil.</summary>
    private void AplicarTipo(TipoOperacion nuevo)
    {
        _tipo = nuevo;
        bool transferencia = nuevo == TipoOperacion.Transferencia;

        LblError.Texto = null;
        LimpiarErrores();
        LblDescripcion.Text = Textos.DescripcionOperacion(nuevo);

        CampoCuenta.IsVisible = _controlador.PideCuenta;
        CampoCuenta.Titulo = transferencia ? Textos.OpCuentaOrigen : Textos.OpCuenta;
        CampoDestino.IsVisible = _controlador.PideDestino(nuevo);
        CampoClave.IsVisible = _controlador.PideClave(nuevo);
        CampoClave.Titulo = transferencia ? Textos.OpClaveCuentaOrigen : Textos.OpClaveCuenta;
        CampoImporte.TipoRetorno = _controlador.PideClave(nuevo) ? ReturnType.Next : ReturnType.Done;

        BtnRegistrar.Texto = Textos.VerboOperacion(nuevo);
        BtnRegistrar.Imagen = Textos.ImagenOperacion(nuevo);
    }

    private async Task CargarCuentaClienteAsync()
    {
        if (_cargandoCuenta)
            return;
        _cargandoCuenta = true;
        PrgOrigen.IsVisible = PrgOrigen.IsRunning = true;

        Resultado<Cuenta> resultado;
        try
        {
            resultado = await _controlador.CargarCuentaClienteAsync();
        }
        catch (Exception)
        {
            resultado = Resultado<Cuenta>.Error(Textos.ErrorInesperado);
        }
        _cargandoCuenta = false;
        PrgOrigen.IsVisible = PrgOrigen.IsRunning = false;

        if (resultado.SesionExpirada)
            await VolverAlLoginAsync(resultado.Mensaje);
        else if (resultado.Exito)
            MostrarCuentaOrigen(resultado.Datos);
    }

    private void MostrarCuentaOrigen(Cuenta? cuenta)
    {
        LblCuentaOrigen.Text = cuenta?.Numero ?? _controlador.CuentaSesion;
        LblSaldoOrigen.Text = cuenta is null
            ? ""
            : string.Format(Textos.OpSaldoDisponible, Formato.Importe(cuenta.Saldo, cuenta.Moneda));
        LblSaldoOrigen.IsVisible = cuenta is not null;
    }

    private async Task AlRegistrarAsync()
    {
        if (_registrando || _tipo is not TipoOperacion tipo)
            return;
        LblError.Texto = null;

        string cuenta = CampoCuenta.Texto;
        string destino = CampoDestino.Texto;
        string importe = CampoImporte.Texto;
        string clave = CampoClave.Texto;

        ValidacionResultado validacion = _controlador.Validar(tipo, cuenta, destino, importe, clave);
        if (!validacion.Valido)
        {
            MostrarErrorCampo(validacion);
            return;
        }

        QuitarFoco();
        SolicitudOperacion solicitud = _controlador.Preparar(tipo, cuenta, destino, importe, clave);
        if (await ConfirmarAsync(solicitud))
            await EnviarAsync(solicitud);
    }

    private void MostrarErrorCampo(ValidacionResultado validacion)
    {
        switch (validacion.Campo)
        {
            case Campo.Cuenta:
                CampoCuenta.MostrarError(validacion.Mensaje);
                break;
            case Campo.Destino:
                CampoDestino.MostrarError(validacion.Mensaje);
                break;
            case Campo.Importe:
                CampoImporte.MostrarError(validacion.Mensaje);
                break;
            case Campo.Clave:
                CampoClave.MostrarError(validacion.Mensaje);
                break;
            default:
                LblError.Texto = validacion.Mensaje;
                break;
        }
    }

    /// <summary>ECUD3 pasos 4 y 5: resumen de la operación para que el usuario la confirme.</summary>
    private Task<bool> ConfirmarAsync(SolicitudOperacion solicitud)
    {
        string importe = Formato.Importe(solicitud.Importe, MonedaConocida());
        string resumen = solicitud.Tipo switch
        {
            TipoOperacion.Deposito => string.Format(Textos.OpConfirmarDeposito, importe, solicitud.Cuenta),
            TipoOperacion.Retiro => string.Format(Textos.OpConfirmarRetiro, importe, solicitud.Cuenta),
            _ => string.Format(Textos.OpConfirmarTransferencia, importe, solicitud.Cuenta, solicitud.CuentaDestino),
        };
        return DisplayAlertAsync(Textos.OpConfirmarTitulo, resumen, Textos.OpConfirmar, Textos.Cancelar);
    }

    private async Task EnviarAsync(SolicitudOperacion solicitud)
    {
        _registrando = true;
        MostrarCargando(true);

        Resultado<Comprobante> resultado;
        try
        {
            resultado = await _controlador.RegistrarAsync(solicitud);
        }
        catch (Exception)
        {
            resultado = Resultado<Comprobante>.Error(Textos.ErrorInesperado);
        }
        _registrando = false;
        MostrarCargando(false);

        if (resultado.SesionExpirada)
        {
            await VolverAlLoginAsync(resultado.Mensaje);
            return;
        }
        if (!resultado.Exito)
        {
            // ECUD3 FA3: ante una clave incorrecta se borra el campo de clave; el resto se conserva.
            if (resultado.Mensaje.Contains("clave", StringComparison.OrdinalIgnoreCase))
                CampoClave.Texto = "";
            LblError.Texto = resultado.Mensaje;
            return;
        }

        string? moneda = MonedaConocida();
        LimpiarFormulario();
        if (!_controlador.PideCuenta)
            MostrarCuentaOrigen(_controlador.CuentaClienteGuardada);
        await Navigation.PushModalAsync(new ComprobantePage(resultado.Mensaje, resultado.Datos!, moneda));
    }

    private string? MonedaConocida()
        => _controlador.PideCuenta ? null : _controlador.CuentaClienteGuardada?.Moneda;

    private void LimpiarFormulario()
    {
        CampoCuenta.Texto = "";
        CampoDestino.Texto = "";
        CampoImporte.Texto = "";
        CampoClave.Texto = "";
        LimpiarErrores();
    }

    private void LimpiarErrores()
    {
        CampoCuenta.Error = null;
        CampoDestino.Error = null;
        CampoImporte.Error = null;
        CampoClave.Error = null;
    }

    private void QuitarFoco()
    {
        CampoCuenta.QuitarFoco();
        CampoDestino.QuitarFoco();
        CampoImporte.QuitarFoco();
        CampoClave.QuitarFoco();
    }

    /// <summary>RNF-013: bloquea los controles mientras se procesa la solicitud.</summary>
    private void MostrarCargando(bool activo)
    {
        BtnRegistrar.Cargando = activo;
        GrupoTipo.IsEnabled = !activo;
        CampoCuenta.IsEnabled = !activo;
        CampoDestino.IsEnabled = !activo;
        CampoImporte.IsEnabled = !activo;
        CampoClave.IsEnabled = !activo;
    }
}
