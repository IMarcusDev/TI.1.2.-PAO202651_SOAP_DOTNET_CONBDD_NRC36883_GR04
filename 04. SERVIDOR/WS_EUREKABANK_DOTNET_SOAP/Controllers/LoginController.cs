using WS_EUREKABANK_DOTNET_SOAP.Models.Data;
using WS_EUREKABANK_DOTNET_SOAP.Models.Interfaces;
using WS_EUREKABANK_DOTNET_SOAP.Models.Repositories;
//revisado y aprovado por Marcos Escobar
using Microsoft.Data.SqlClient;
using WS_EUREKABANK_DOTNET_SOAP.Services;

namespace WS_EUREKABANK_DOTNET_SOAP.Controllers
{
  public class LoginController
  {
    private static readonly ILoginRepository repository = new LoginImplRepository();
    //revisado y aprovado por Marcos Escobar
    private static readonly ICuentaRepository cuentaRepository = new CuentaImplRepository();

    //revisado y aprovado por Marcos Escobar
    public const string CREDENCIALES_INCORRECTAS = "Credenciales incorrectas";
    public const string SERVICIO_NO_DISPONIBLE = "Servicio no disponible, intente más tarde";
    public const string USUARIO_INACTIVO = "El usuario no se encuentra activo";
    public const string CUENTA_INACTIVA = "La cuenta no se encuentra activa";
    public const string ACTIVO = "ACTIVO";

    public static Usuario GetUsuarioById(string id)
    {
      return LoginController.repository.GetUsuario(id);
    }

    public static Perfil GetPerfil(string username)
    {
      return LoginController.repository.GetPerfil(username);
    }

    //revisado y aprovado por Marcos Escobar
    // Login general (operación Login): usuario y clave de cualquier perfil.
    public static Usuario Autenticar(string? username, string? password)
    {
      Usuario u = VerificarCredenciales(username, null, password);

      if (!EsActivo(u.Estado))
        throw new Exception(USUARIO_INACTIVO);

      return u;
    }

    //revisado y aprovado por Marcos Escobar
    // loginEmpleado: solo entra un usuario de tipo Empleado y activo.
    public static Usuario AutenticarEmpleado(string? usuario, string? clave)
    {
      Usuario u = VerificarCredenciales(usuario, null, clave);

      if (u.Tipo != "Empleado")
        throw new Exception(CREDENCIALES_INCORRECTAS);
      if (!EsActivo(u.Estado))
        throw new Exception(USUARIO_INACTIVO);

      return u;
    }

    //revisado y aprovado por Marcos Escobar
    // loginCliente: número de cuenta (que es el ID del perfil) y su clave; la cuenta debe estar ACTIVO.
    public static Usuario AutenticarCliente(string? cuenta, string? clave)
    {
      Usuario u = VerificarCredenciales(null, cuenta, clave);
      Cuenta c;

      try
      {
        c = cuentaRepository.GetCuenta(cuenta!);
      }
      catch (SqlException)
      {
        throw new Exception(SERVICIO_NO_DISPONIBLE);
      }
      catch (Exception)
      {
        throw new Exception(CREDENCIALES_INCORRECTAS);
      }

      if (!EsActivo(c.Estado))
        throw new Exception(CUENTA_INACTIVA);

      return u;
    }

    //revisado y aprovado por Marcos Escobar
    public static string PerfilDe(Usuario u)
    {
      return u.Tipo == "Empleado" ? Sesion.EMPLEADO : Sesion.CLIENTE;
    }

    //revisado y aprovado por Marcos Escobar
    public static bool EsActivo(string? estado)
    {
      return string.Equals(estado, ACTIVO, StringComparison.OrdinalIgnoreCase);
    }

    //revisado y aprovado por Marcos Escobar
    // Busca el perfil por usuario o por ID y compara la clave. Cualquier dato que no
    // coincida da el mismo mensaje, para no revelar qué usuarios o cuentas existen.
    private static Usuario VerificarCredenciales(string? username, string? id, string? clave)
    {
      Perfil p;
      Usuario u;

      if (string.IsNullOrEmpty(username) && string.IsNullOrEmpty(id))
        throw new Exception(CREDENCIALES_INCORRECTAS);

      try
      {
        p = username != null ? repository.GetPerfil(username) : repository.GetPerfilById(id!);
        u = repository.GetUsuario(p.Id);
      }
      catch (SqlException)
      {
        throw new Exception(SERVICIO_NO_DISPONIBLE);
      }
      catch (Exception)
      {
        throw new Exception(CREDENCIALES_INCORRECTAS);
      }

      if (clave == null || !HashService.VerifyPassword(clave, p.PasswordHash))
        throw new Exception(CREDENCIALES_INCORRECTAS);

      return u;
    }
  }
}
