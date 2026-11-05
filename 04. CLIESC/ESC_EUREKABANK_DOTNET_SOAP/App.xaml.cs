using System.Net.Http;
using System.Windows;
using ec.edu.monster.config;
using ec.edu.monster.controlador;
using ec.edu.monster.modelo;
using ec.edu.monster.modelo.servicios;
using ec.edu.monster.modelo.soap;
using ec.edu.monster.vista;
using ec.edu.monster.vista.modelos;

namespace ec.edu.monster;

/// <summary>
/// Composition root: builds the configuration, the SOAP transport, the services and the session,
/// and moves between the login window and the main window.
/// </summary>
public partial class App : Application
{
    private HttpClient? _http;
    private HttpClient? _httpEstado;
    private Sesion? _sesion;
    private LoginServicio? _login;
    private MovimientoServicio? _movimientos;
    private EstadoServidor? _estado;
    private Window? _ventana;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Configuracion configuracion;
        try
        {
            configuracion = Configuracion.Cargar();
        }
        catch (Exception error)
        {
            MessageBox.Show(
                $"No se pudo leer {Configuracion.Archivo}: {error.Message}",
                "EurekaBank",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
            return;
        }

        _http = new HttpClient { Timeout = RutasSoap.TiempoEspera };
        _httpEstado = new HttpClient { Timeout = RutasSoap.TiempoEsperaEstado };

        var transporte = new TransporteSoap(_http);
        _login = new LoginServicio(transporte, configuracion);
        _movimientos = new MovimientoServicio(transporte, configuracion);
        _sesion = new Sesion();
        _estado = new EstadoServidor(configuracion, _httpEstado);

        MostrarLogin();
    }

    private void MostrarLogin()
    {
        var controlador = new LoginControlador(_login!, _sesion!);
        var modelo = new LoginViewModel(controlador, MostrarPrincipal);
        Abrir(new LoginWindow(modelo, NuevoEstadoViewModel()));
    }

    private void MostrarPrincipal()
    {
        var controlador = new LoginControlador(_login!, _sesion!);
        Abrir(new PrincipalWindow(_sesion!, NuevoEstadoViewModel(), async () =>
        {
            await controlador.CerrarSesionAsync();
            MostrarLogin();
        }));
    }

    private EstadoServidorViewModel NuevoEstadoViewModel() => new(_estado!);

    /// <summary>Shows the new window before closing the old one, so the application never flickers out.</summary>
    private void Abrir(Window ventana)
    {
        Window? anterior = _ventana;
        _ventana = ventana;
        MainWindow = ventana;
        ventana.Show();
        anterior?.Close();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _http?.Dispose();
        _httpEstado?.Dispose();
        base.OnExit(e);
    }
}
