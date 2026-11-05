using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using ec.edu.monster.vista.modelos;

namespace ec.edu.monster.vista;

/// <summary>
/// Movement form. The PasswordBox cannot be bound, so its content is copied to the view model
/// and cleared back when the view model clears the form.
/// </summary>
public partial class OperacionView : UserControl
{
    private OperacionViewModel? _modelo;

    public OperacionView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => Enganchar(DataContext as OperacionViewModel);
    }

    private void Enganchar(OperacionViewModel? modelo)
    {
        if (_modelo is not null)
            _modelo.PropertyChanged -= ModeloCambio;

        _modelo = modelo;

        if (_modelo is not null)
            _modelo.PropertyChanged += ModeloCambio;
    }

    private void ModeloCambio(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(OperacionViewModel.Clave) || _modelo is null)
            return;
        if (CampoClave.Password != _modelo.Clave)
            CampoClave.Password = _modelo.Clave;
    }

    private void ClaveCambiada(object sender, RoutedEventArgs e)
    {
        if (_modelo is not null)
            _modelo.Clave = CampoClave.Password;
    }
}
