using MOV_EUREKABANK_DOTNET_SOAP.Config;
using MOV_EUREKABANK_DOTNET_SOAP.Controlador;
using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Data;
using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Enums;
using MOV_EUREKABANK_DOTNET_SOAP.Vista.Componentes;

namespace MOV_EUREKABANK_DOTNET_SOAP.Vista;

/// <summary>ECUD1 Iniciar Sesión: selector de perfil y credenciales según el perfil.</summary>
public partial class LoginPage : ContentPage
{
    private readonly LoginControlador _controlador;
    private Perfil _perfil = Perfil.Empleado;
    private bool _cargando;

    public LoginPage(LoginControlador controlador)
    {
        InitializeComponent();
        _controlador = controlador;

        GrupoPerfil.Configurar(
        [
            new Segmentos.Opcion(Perfil.Empleado, "Empleado", Iconos.Persona),
            new Segmentos.Opcion(Perfil.Cliente, "Cliente", Iconos.Tarjeta),
        ]);
        GrupoPerfil.Seleccion = Perfil.Empleado;
        GrupoPerfil.SeleccionCambiada += (_, perfil) => AplicarPerfil((Perfil)perfil);
        AplicarPerfil(Perfil.Empleado);

        CampoCredencial.Completado += (_, _) => CampoClave.Enfocar();
        CampoClave.Completado += async (_, _) => await AlIngresarAsync();
        BtnIngresar.Pulsado += async (_, _) => await AlIngresarAsync();

        LblServidor.Text = string.Format(Textos.LoginServidor, new Uri(SoapConfig.UrlBase).Authority);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_controlador.HaySesion)
            await Shell.Current.GoToAsync(NavegacionSesion.RutaPrincipal);
    }

    /// <summary>El empleado usa usuario y clave; el cliente, número de cuenta (8 dígitos) y clave (6 dígitos).</summary>
    private void AplicarPerfil(Perfil nuevo)
    {
        _perfil = nuevo;
        LblError.Texto = null;
        CampoCredencial.Texto = "";
        CampoClave.Texto = "";
        CampoCredencial.Error = null;
        CampoClave.Error = null;

        if (nuevo == Perfil.Cliente)
        {
            CampoCredencial.Titulo = Textos.LoginCuenta;
            CampoCredencial.IconoDatos = Iconos.Tarjeta;
            CampoCredencial.Ayuda = Textos.LoginAyudaCuenta;
            CampoCredencial.Teclado = Keyboard.Numeric;
            CampoCredencial.LargoMaximo = ValidadorDatos.LargoCuenta;

            CampoClave.Titulo = Textos.LoginClaveCuenta;
            CampoClave.Ayuda = Textos.LoginAyudaClaveCuenta;
            CampoClave.Teclado = Keyboard.Numeric;
            CampoClave.LargoMaximo = ValidadorDatos.LargoClaveCuenta;
        }
        else
        {
            CampoCredencial.Titulo = Textos.LoginUsuario;
            CampoCredencial.IconoDatos = Iconos.Persona;
            CampoCredencial.Ayuda = null;
            CampoCredencial.Teclado = Keyboard.Plain;
            CampoCredencial.LargoMaximo = ValidadorDatos.MaxUsuario;

            CampoClave.Titulo = Textos.LoginClave;
            CampoClave.Ayuda = null;
            CampoClave.Teclado = Keyboard.Default;
            CampoClave.LargoMaximo = ValidadorDatos.MaxClaveEmpleado;
        }
        // Al cambiar de perfil se vuelve a ocultar la clave.
        CampoClave.EsClave = false;
        CampoClave.EsClave = true;
    }

    private async Task AlIngresarAsync()
    {
        if (_cargando)
            return;

        Perfil perfilElegido = _perfil;
        string credencial = CampoCredencial.Texto;
        string clave = CampoClave.Texto;
        LblError.Texto = null;

        ValidacionResultado validacion = _controlador.Validar(perfilElegido, credencial, clave);
        if (!validacion.Valido)
        {
            (validacion.Campo == Campo.Clave ? CampoClave : CampoCredencial).MostrarError(validacion.Mensaje);
            return;
        }

        CampoCredencial.QuitarFoco();
        CampoClave.QuitarFoco();
        MostrarCargando(true);
        Resultado<Sesion> resultado;
        try
        {
            resultado = await _controlador.IniciarSesionAsync(perfilElegido, credencial, clave);
        }
        catch (Exception)
        {
            resultado = Resultado<Sesion>.Error(Textos.ErrorInesperado);
        }
        MostrarCargando(false);

        if (resultado.Exito)
        {
            await Shell.Current.GoToAsync(NavegacionSesion.RutaPrincipal);
        }
        else
        {
            // ECUD1 FA1: se muestra el mensaje y se borra la clave para un nuevo intento.
            CampoClave.Texto = "";
            LblError.Texto = resultado.Mensaje;
        }
    }

    private void MostrarCargando(bool cargando)
    {
        _cargando = cargando;
        BtnIngresar.Cargando = cargando;
        CampoCredencial.IsEnabled = !cargando;
        CampoClave.IsEnabled = !cargando;
        GrupoPerfil.IsEnabled = !cargando;
    }
}
