using System.Windows;
using ec.edu.monster.vista.modelos;

namespace ec.edu.monster.vista;

/// <summary>
/// Main window: header with the logged-in user, the sidebar navigation, the screen host and the
/// server status bar. The screen itself is rendered from PrincipalViewModel.Contenido.
/// </summary>
public partial class PrincipalWindow : Window
{
    private readonly Func<Task> _cerrarSesion;

    public PrincipalWindow(PrincipalViewModel modelo, Func<Task> cerrarSesion)
    {
        InitializeComponent();
        _cerrarSesion = cerrarSesion;

        DataContext = modelo;
        IndicadorEstadoControl.DataContext = modelo.Estado;

        Loaded += async (_, _) => await modelo.Estado.IniciarAsync();
        Closed += (_, _) => modelo.Estado.Detener();
    }

    private async void CerrarSesion(object sender, RoutedEventArgs e)
        => await _cerrarSesion();
}
