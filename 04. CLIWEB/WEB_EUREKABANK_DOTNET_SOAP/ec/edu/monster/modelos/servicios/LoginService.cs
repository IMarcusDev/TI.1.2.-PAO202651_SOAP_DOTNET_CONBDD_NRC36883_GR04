using ec.edu.monster.config;
using ec.edu.monster.modelos.dto;
using ec.edu.monster.modelos.soap;
using Microsoft.Extensions.Options;

namespace ec.edu.monster.modelos.servicios;

/// <summary>
/// Client-side access to the SOAP LoginWebService. Controllers use this class and never build
/// envelopes themselves. Every failure comes back as Ok = false with a message to show.
/// </summary>
public class LoginService(ITransporte transporte, IOptions<ServidorOptions> opciones)
    : ServicioSoap(transporte, opciones, RutasSoap.ServicioLogin, RutasSoap.ContratoLogin)
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
                respuesta.Texto("tipoUsuario") ?? "");
        }
        catch (Exception e)
        {
            return LoginResultado.Error(Mensaje(e));
        }
    }
}
