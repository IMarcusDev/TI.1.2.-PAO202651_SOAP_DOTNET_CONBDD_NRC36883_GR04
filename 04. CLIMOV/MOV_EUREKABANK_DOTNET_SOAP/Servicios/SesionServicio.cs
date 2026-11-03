using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Data;

namespace MOV_EUREKABANK_DOTNET_SOAP.Servicios;

/// <summary>Guarda el token y el perfil mientras dura la sesión, y la última información conocida de la cuenta del cliente.</summary>
public class SesionServicio
{
    private volatile Sesion? _sesion;
    private volatile Cuenta? _cuentaCliente;

    public Sesion? Sesion => _sesion;

    public string? Token => _sesion?.Token;

    public bool EstaAutenticado => !string.IsNullOrWhiteSpace(Token);

    public Cuenta? CuentaCliente => _cuentaCliente;

    public void Iniciar(Sesion nueva)
    {
        _sesion = nueva;
        _cuentaCliente = null;
    }

    public void Limpiar()
    {
        _sesion = null;
        _cuentaCliente = null;
    }

    public void ActualizarCuentaCliente(Cuenta cuenta) => _cuentaCliente = cuenta;

    public void ActualizarSaldoCliente(decimal? saldo)
    {
        Cuenta? actual = _cuentaCliente;
        if (actual is not null && saldo is decimal nuevo)
            _cuentaCliente = actual.ConSaldo(nuevo);
    }
}
