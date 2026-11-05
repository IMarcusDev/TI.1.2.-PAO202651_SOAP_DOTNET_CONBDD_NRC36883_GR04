using ec.edu.monster.config;
using ec.edu.monster.modelo.dto;
using ec.edu.monster.modelo.soap;

namespace ec.edu.monster.modelo.servicios;

/// <summary>
/// Client-side access to the SOAP LoginWebService. Controllers use this class and never build
/// envelopes themselves. Every failure comes back as Ok = false with a message to show.
/// </summary>
public class LoginServicio(ITransporte transporte, Configuracion configuracion)
    : ServicioSoap(transporte, configuracion, RutasSoap.ServicioLogin, RutasSoap.ContratoLogin)
{
    public const string CredencialesIncorrectas = "Credenciales incorrectas";

    /// <summary>Employee (bank teller) login with username and key.</summary>
    public Task<LoginResultado> LoginEmpleadoAsync(string usuario, string clave)
        => LoginAsync(Peticion("loginEmpleado").Parametro("usuario", usuario).Parametro("clave", clave));

    /// <summary>Cliente login with the account number and its key; the token is tied to that account.</summary>
    public Task<LoginResultado> LoginClienteAsync(string cuenta, string clave)
        => LoginAsync(Peticion("loginCliente").Parametro("cuenta", cuenta).Parametro("clave", clave));

    /// <summary>Ends the token on the server. A failure is ignored by the caller: the local session ends anyway.</summary>
    public async Task<bool> LogoutAsync(string token)
    {
        try
        {
            return (await EnviarAsync(Peticion("logout").Parametro("token", token))).EsOk();
        }
        catch (Exception)
        {
            return false;
        }
    }

    private async Task<LoginResultado> LoginAsync(SoapConstructorPeticion peticion)
    {
        try
        {
            SoapLectorRespuesta respuesta = await EnviarAsync(peticion);
            if (respuesta.Estado() is null)
                return LoginResultado.Error(RespuestaIncompleta);
            if (!respuesta.EsOk())
                return LoginResultado.Error(MensajeServidor(respuesta.Texto("mensaje"), CredencialesIncorrectas));

            string? token = respuesta.Texto("token");
            if (string.IsNullOrEmpty(token))
                return LoginResultado.Error(RespuestaIncompleta);

            return new LoginResultado(true,
                respuesta.Texto("mensaje") ?? "",
                token,
                respuesta.Texto("nombre") ?? "",
                TipoUsuario(respuesta.Texto("tipoUsuario"), respuesta.Texto("perfil")));
        }
        catch (Exception e)
        {
            return LoginResultado.Error(Mensaje(e));
        }
    }

    /// <summary>
    /// The .NET server sends TipoUsuario ("Empleado" / "Cliente"); when only the legacy Perfil
    /// ("EMPLEADO" / "CLIENTE") comes, it is normalized to the exact role name the UI compares.
    /// </summary>
    private static string TipoUsuario(string? tipo, string? perfil)
    {
        string valor = string.IsNullOrWhiteSpace(tipo) ? (perfil ?? "").Trim() : tipo.Trim();
        if (valor.Equals(Sesion.RolCliente, StringComparison.OrdinalIgnoreCase)
            || valor.Equals("CLIENTE", StringComparison.OrdinalIgnoreCase))
            return Sesion.RolCliente;
        if (valor.Equals(Sesion.RolEmpleado, StringComparison.OrdinalIgnoreCase)
            || valor.Equals("EMPLEADO", StringComparison.OrdinalIgnoreCase))
            return Sesion.RolEmpleado;
        return valor;
    }
}
