using CONSOLE_EUREKABANK_DOTNET_SOAP.Models.DTO;
using CONSOLE_EUREKABANK_DOTNET_SOAP.Controllers;
using CONSOLE_EUREKABANK_DOTNET_SOAP.Models.HTTP;
using CONSOLE_EUREKABANK_DOTNET_SOAP.Views.Utils;

namespace CONSOLE_EUREKABANK_DOTNET_SOAP.Views
{
  /// <summary>ECUD1 paso 7: menú del perfil con «Consultar movimientos», «Registrar movimiento» y «Cerrar sesión».</summary>
  //revisado y aprovado por Marcos Escobar
  internal class MenuView
  {
    public static async Task Show()
    {
      while (ConfigHTTP.HaySesion())
      {
        MessagesUtils.PrintTitle("MENU PRINCIPAL");
        Console.WriteLine($"{ConfigHTTP.Nombre} - {(ConfigHTTP.EsCliente() ? "Cliente, cuenta " + ConfigHTTP.Cuenta : "Empleado")}\n");

        Console.WriteLine("1. Consultar movimientos");
        Console.WriteLine("2. Registrar movimiento");
        Console.WriteLine("3. Cerrar sesion");

        string opcion = MessagesUtils.ReadText("\nSeleccione una opcion");

        switch (opcion)
        {
          case "1":
            await ConsultaView.Show();
            break;

          case "2":
            await MovimientoView.Show();
            break;

          case "3":
            await CerrarSesion();
            break;

          default:
            MessagesUtils.PrintError("Opcion invalida");
            break;
        }

        Console.WriteLine();
      }
    }

    /// <summary>ECUD1 alterno 5: se borra la sesión y se vuelve al inicio aunque el servidor no responda.</summary>
    private static async Task CerrarSesion()
    {
      ResponseDTO res = await new LoginController().LogoutAsync();
      MessagesUtils.PrintMessage(res.GetEstado() == CONSOLE_EUREKABANK_DOTNET_SOAP.Models.Enums.EstadosEnum.OK
        ? res.GetMensaje()
        : "Sesion cerrada");
    }

    /// <summary>ECUD4: con el token vencido se informa y se vuelve al inicio de sesión.</summary>
    public static bool RevisarSesion(ResponseDTO res)
    {
      if (!res.SesionExpirada())
        return false;

      MessagesUtils.PrintError(ErroresCliente.SesionExpirada);
      ConfigHTTP.Limpiar();
      return true;
    }
  }
}
