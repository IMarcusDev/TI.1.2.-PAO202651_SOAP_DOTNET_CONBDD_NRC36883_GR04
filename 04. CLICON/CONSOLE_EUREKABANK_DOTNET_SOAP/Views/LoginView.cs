using CONSOLE_EUREKABANK_DOTNET_SOAP.Models.DTO;
using CONSOLE_EUREKABANK_DOTNET_SOAP.Models.Enums;
using CONSOLE_EUREKABANK_DOTNET_SOAP.Controllers;
using CONSOLE_EUREKABANK_DOTNET_SOAP.Views.Utils;

namespace CONSOLE_EUREKABANK_DOTNET_SOAP.Views
{
  internal class LoginView
  {
    //revisado y aprovado por Marcos Escobar
    // ECUD1: el usuario elige su perfil e ingresa sus credenciales. Devuelve false si elige salir.
    public static bool UserHasLogin()
    {
      var ctrl = new LoginController();

      while (true)
      {
        MessagesUtils.PrintTitle("INICIAR SESION");

        Console.WriteLine("1. Empleado");
        Console.WriteLine("2. Cliente");
        Console.WriteLine("3. Salir");

        string opcion = MessagesUtils.ReadText("\nSeleccione su perfil");

        LoginResponseDTO? res;

        switch (opcion)
        {
          case "1":
            res = LoginEmpleado(ctrl);
            break;

          case "2":
            res = LoginCliente(ctrl);
            break;

          case "3":
            return false;

          default:
            MessagesUtils.PrintError("Opcion invalida");
            continue;
        }

        if (res is null)
          continue;

        if (res.estado == EstadosEnum.OK)
        {
          MessagesUtils.PrintMessage(res.mensaje ?? string.Empty);
          Thread.Sleep(TimeSpan.FromSeconds(1));
          return true;
        }

        // ECUD1 alterno 1: se muestra el mensaje y se mantiene el formulario para otro intento.
        MessagesUtils.PrintError(res.mensaje ?? string.Empty);
        Console.WriteLine();
      }
    }

    //revisado y aprovado por Marcos Escobar
    private static LoginResponseDTO? LoginEmpleado(LoginController ctrl)
    {
      // Username
      string usuario = MessagesUtils.ReadText("Usuario");
      if (usuario.Length == 0)
      {
        MessagesUtils.PrintError(ValidadorDatos.IngreseUsuario);
        return null;
      }

      // Password
      string clave = MessagesUtils.ReadPassword("Clave");
      if (clave.Length == 0)
      {
        MessagesUtils.PrintError(ValidadorDatos.IngreseClave);
        return null;
      }

      MessagesUtils.PrintMessage("Procesando...");
      return ctrl.LoginEmpleadoAsync(usuario, clave).Result;
    }

    //revisado y aprovado por Marcos Escobar
    private static LoginResponseDTO? LoginCliente(LoginController ctrl)
    {
      string cuenta = MessagesUtils.ReadText("Numero de cuenta");
      string? error = ValidadorDatos.ErrorCuenta(cuenta);
      if (error is not null)
      {
        MessagesUtils.PrintError(error);
        return null;
      }

      string clave = MessagesUtils.ReadPassword("Clave");
      error = ValidadorDatos.ErrorClaveCuenta(clave);
      if (error is not null)
      {
        MessagesUtils.PrintError(error);
        return null;
      }

      MessagesUtils.PrintMessage("Procesando...");
      return ctrl.LoginClienteAsync(cuenta, clave).Result;
    }
  }
}
