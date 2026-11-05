using System.ComponentModel;
using System.Windows.Input;
using ec.edu.monster.controlador;

namespace ec.edu.monster.vista.modelos;

/// <summary>State of the login form: role, credentials, error message and the busy flag.</summary>
public sealed class LoginViewModel : INotifyPropertyChanged
{
    private readonly LoginControlador _controlador;
    private readonly Action _alIngresar;
    private readonly AsyncRelayCommand _ingresar;

    private bool _esCliente;
    private string _usuario = "";
    private string _clave = "";
    private string _error = "";
    private bool _ocupado;

    public LoginViewModel(LoginControlador controlador, Action alIngresar)
    {
        _controlador = controlador;
        _alIngresar = alIngresar;
        _ingresar = new AsyncRelayCommand(IngresarAsync, () => !Ocupado);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ICommand IngresarCommand => _ingresar;

    /// <summary>True when the user picked "Cliente" (account number + key).</summary>
    public bool EsCliente
    {
        get => _esCliente;
        set
        {
            if (_esCliente == value)
                return;
            _esCliente = value;
            OnPropertyChanged(nameof(EsCliente));
            OnPropertyChanged(nameof(EsEmpleado));
            OnPropertyChanged(nameof(EtiquetaUsuario));
        }
    }

    /// <summary>Inverse of EsCliente, bound to the "Empleado" radio button.</summary>
    public bool EsEmpleado
    {
        get => !_esCliente;
        set
        {
            if (value)
                EsCliente = false;
        }
    }

    public string EtiquetaUsuario => EsCliente ? "Número de cuenta" : "Usuario";

    public string Usuario
    {
        get => _usuario;
        set
        {
            if (_usuario == value)
                return;
            _usuario = value;
            OnPropertyChanged(nameof(Usuario));
        }
    }

    public string Clave
    {
        get => _clave;
        set
        {
            if (_clave == value)
                return;
            _clave = value;
            OnPropertyChanged(nameof(Clave));
        }
    }

    public string Error
    {
        get => _error;
        private set
        {
            if (_error == value)
                return;
            _error = value;
            OnPropertyChanged(nameof(Error));
            OnPropertyChanged(nameof(HayError));
        }
    }

    public bool HayError => !string.IsNullOrEmpty(_error);

    public bool Ocupado
    {
        get => _ocupado;
        private set
        {
            if (_ocupado == value)
                return;
            _ocupado = value;
            OnPropertyChanged(nameof(Ocupado));
            OnPropertyChanged(nameof(PuedeIngresar));
            _ingresar.RaiseCanExecuteChanged();
        }
    }

    public bool PuedeIngresar => !_ocupado;

    public async Task IngresarAsync()
    {
        if (Ocupado)
            return;

        Ocupado = true;
        Error = "";
        try
        {
            string? error = await _controlador.IngresarAsync(Usuario, Clave, EsCliente);
            if (error is not null)
            {
                Error = error;
                return;
            }
            Clave = "";
            _alIngresar();
        }
        finally
        {
            Ocupado = false;
        }
    }

    private void OnPropertyChanged(string nombre)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombre));
}
