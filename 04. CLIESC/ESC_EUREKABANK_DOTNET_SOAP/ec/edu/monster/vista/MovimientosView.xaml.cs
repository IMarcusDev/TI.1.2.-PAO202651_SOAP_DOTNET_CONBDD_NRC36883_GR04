using System.Windows;
using System.Windows.Controls;
using ec.edu.monster.vista.modelos;

namespace ec.edu.monster.vista;

/// <summary>
/// "Consultar movimientos" screen. A Cliente cannot type an account, so the history is requested
/// as soon as the screen is shown.
/// </summary>
public partial class MovimientosView : UserControl
{
    public MovimientosView()
    {
        InitializeComponent();
    }

    private async void AlCargar(object sender, RoutedEventArgs e)
    {
        if (DataContext is MovimientosViewModel { EsCliente: true } modelo)
            await modelo.ConsultarAsync();
    }
}
