using System.Windows;
using ec.edu.monster.modelo;

namespace ec.edu.monster.vista;

/// <summary>Modal receipt shown after the server accepts a deposit, withdrawal or transfer.</summary>
public partial class ComprobanteWindow : Window
{
    public ComprobanteWindow(Comprobante comprobante)
    {
        InitializeComponent();
        Detalle.ItemsSource = comprobante.Filas();
    }

    private void Aceptar(object sender, RoutedEventArgs e) => Close();
}
