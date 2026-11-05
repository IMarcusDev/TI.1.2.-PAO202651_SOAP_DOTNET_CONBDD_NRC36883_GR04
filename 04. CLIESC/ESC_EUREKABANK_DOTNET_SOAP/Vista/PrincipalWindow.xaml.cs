using System.Windows;
using ec.edu.monster.modelo;
using ec.edu.monster.vista.modelos;

namespace ec.edu.monster.vista;

/// <summary>
/// Main window shell: header with the logged-in user, the menu placeholder and the server status
/// bar. The operation screens are added in the next work unit.
/// </summary>
public partial class PrincipalWindow : Window
{
    private readonly Func<Task> _cerrarSesion;
    private readonly EstadoServidorViewModel _estado;

    public PrincipalWindow(Sesion sesion, EstadoServidorViewModel estado, Func<Task> cerrarSesion)
    {
        InitializeComponent();
        _cerrarSesion = cerrarSesion;
        _estado = estado;

        DataContext = sesion;
        IndicadorEstadoControl.DataContext = estado;

        Loaded += async (_, _) => await _estado.IniciarAsync();
        Closed += (_, _) => _estado.Detener();
    }

    private async void CerrarSesion(object sender, RoutedEventArgs e)
        => await _cerrarSesion();
}
