using System.Windows;
using ec.edu.monster.vista.modelos;

namespace ec.edu.monster.vista;

/// <summary>
/// Login window: the form is bound to <see cref="LoginViewModel"/>, and the status bar keeps its
/// own view model so it can poll the server while the user types.
/// </summary>
public partial class LoginWindow : Window
{
    private readonly LoginViewModel _modelo;
    private readonly EstadoServidorViewModel _estado;

    public LoginWindow(LoginViewModel modelo, EstadoServidorViewModel estado)
    {
        InitializeComponent();
        _modelo = modelo;
        _estado = estado;

        DataContext = modelo;
        IndicadorEstadoControl.DataContext = estado;

        Loaded += (_, _) => CampoUsuario.Focus();
        Loaded += async (_, _) => await _estado.IniciarAsync();
        Closed += (_, _) => _estado.Detener();
    }

    private void ClaveCambiada(object sender, RoutedEventArgs e)
        => _modelo.Clave = CampoClave.Password;
}
