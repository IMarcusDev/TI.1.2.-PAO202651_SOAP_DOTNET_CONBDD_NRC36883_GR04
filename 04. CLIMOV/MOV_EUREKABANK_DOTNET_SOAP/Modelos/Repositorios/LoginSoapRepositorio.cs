using MOV_EUREKABANK_DOTNET_SOAP.Config;
using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Data;
using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Enums;
using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Interfaces;
using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Soap;

namespace MOV_EUREKABANK_DOTNET_SOAP.Modelos.Repositorios;

/// <summary>LoginWebService: loginEmpleado, loginCliente y logout (ECUD1).</summary>
public class LoginSoapRepositorio(string urlBase, SoapTransporte transporte, IVerificadorRed red)
    : RepositorioSoap(urlBase, SoapConfig.ServicioLogin, SoapConfig.ContratoLogin, transporte, red), ILoginRepositorio
{
    public const string CredencialesIncorrectas = "Credenciales incorrectas";

    public Task<Resultado<Sesion>> LoginEmpleadoAsync(string usuario, string clave)
        => LoginAsync(Peticion("loginEmpleado")
            .Parametro("usuario", usuario)
            .Parametro("clave", clave), Perfil.Empleado, null);

    public Task<Resultado<Sesion>> LoginClienteAsync(string cuenta, string clave)
        => LoginAsync(Peticion("loginCliente")
            .Parametro("cuenta", cuenta)
            .Parametro("clave", clave), Perfil.Cliente, cuenta);

    private async Task<Resultado<Sesion>> LoginAsync(SoapConstructorPeticion peticion, Perfil perfilPedido, string? cuenta)
    {
        if (SinRed())
            return Resultado<Sesion>.Error(SoapErrores.SinConexion);

        try
        {
            SoapLectorRespuesta respuesta = await EnviarAsync(peticion);
            if (respuesta.Estado() is null)
                return Resultado<Sesion>.Error(SoapErrores.RespuestaIncompleta);
            if (!respuesta.EsOk())
                return Resultado<Sesion>.Error(SoapErrores.MensajeServidor(respuesta.Texto("mensaje"), CredencialesIncorrectas));

            string? token = respuesta.Texto("token");
            if (string.IsNullOrEmpty(token))
                return Resultado<Sesion>.Error(SoapErrores.RespuestaIncompleta);

            Perfil perfil = PerfilExtensions.Desde(respuesta.Texto("perfil"), perfilPedido);
            var sesion = new Sesion(token, perfil, TextoOVacio(respuesta.Texto("nombre")), cuenta);
            return Resultado<Sesion>.Ok(sesion, TextoOVacio(respuesta.Texto("mensaje")));
        }
        catch (Exception e)
        {
            return Resultado<Sesion>.Error(SoapErrores.Mensaje(e));
        }
    }

    public async Task<Resultado<bool>> LogoutAsync(string token)
    {
        if (SinRed())
            return Resultado<bool>.Error(SoapErrores.SinConexion);

        try
        {
            SoapLectorRespuesta respuesta = await EnviarAsync(Peticion("logout").Parametro("token", token));
            return respuesta.EsOk()
                ? Resultado<bool>.Ok(true, TextoOVacio(respuesta.Texto("mensaje")))
                : Resultado<bool>.Error(SoapErrores.MensajeServidor(respuesta.Texto("mensaje"), SoapErrores.RespuestaIncompleta));
        }
        catch (Exception e)
        {
            return Resultado<bool>.Error(SoapErrores.Mensaje(e));
        }
    }
}
