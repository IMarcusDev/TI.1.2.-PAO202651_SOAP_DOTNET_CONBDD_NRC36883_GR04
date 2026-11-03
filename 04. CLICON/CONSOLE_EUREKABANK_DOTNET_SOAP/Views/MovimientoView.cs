
using CONSOLE_EUREKABANK_DOTNET_SOAP.Models.DTO;
using CONSOLE_EUREKABANK_DOTNET_SOAP.Models.Enums;
using CONSOLE_EUREKABANK_DOTNET_SOAP.Controllers;
using CONSOLE_EUREKABANK_DOTNET_SOAP.Models.HTTP;
using CONSOLE_EUREKABANK_DOTNET_SOAP.Views.Utils;

namespace CONSOLE_EUREKABANK_DOTNET_SOAP.Views
{
  internal class MovimientoView
  {
    private static MovimientoController controller = new MovimientoController();

    //revisado y aprovado por Marcos Escobar
    // ECUD3 paso 1: el empleado elige Depósito, Retiro o Transferencia; al cliente solo se le muestran Retiro y Transferencia.
    public static async Task Show()
    {
      string[] opciones = ConfigHTTP.EsCliente()
        ? new[] { "Retiro", "Transferencia" }
        : new[] { "Deposito", "Retiro", "Transferencia" };

      while (ConfigHTTP.HaySesion())
      {
        MessagesUtils.PrintTitle("MENU MOVIMIENTOS");

        for (int i = 0; i < opciones.Length; i++)
          Console.WriteLine($"{i + 1}. {opciones[i]}");
        Console.WriteLine($"{opciones.Length + 1}. Regresar");

        if (!int.TryParse(MessagesUtils.ReadText("\nSeleccione una opcion"), out int opcion) || opcion < 1 || opcion > opciones.Length + 1)
        {
          MessagesUtils.PrintError("Opcion invalida");
          continue;
        }

        if (opcion == opciones.Length + 1)
        {
          MessagesUtils.PrintMessage("Regresando...");
          return;
        }

        switch (opciones[opcion - 1])
        {
          case "Deposito":
            await Deposito();
            break;

          case "Retiro":
            await Retiro();
            break;

          case "Transferencia":
            await Transferencia();
            break;
        }

        Console.WriteLine();
      }
    }

    //revisado y aprovado por Marcos Escobar
    private static async Task Deposito()
    {
      MessagesUtils.PrintTitle("DEPOSITO");

      string? cuenta = PedirCuenta("Numero de cuenta");
      if (cuenta is null)
        return;

      decimal? importe = PedirImporte();
      if (importe is null)
        return;

      if (!MessagesUtils.Confirm($"¿Depositar {Formato.Importe(importe, "USD")} en la cuenta {cuenta}?"))
      {
        MessagesUtils.PrintMessage("Operacion cancelada");
        return;
      }

      MessagesUtils.PrintMessage("Procesando...");
      MovimientoResponseDTO response = await controller.DepositoAsync(cuenta, importe.Value);

      PrintResponse(response);
    }

    //revisado y aprovado por Marcos Escobar
    private static async Task Retiro()
    {
      MessagesUtils.PrintTitle("RETIRO");

      string? cuenta;
      string clave = "";

      if (ConfigHTTP.EsCliente())
      {
        cuenta = ConfigHTTP.Cuenta;
        await MostrarCuentaPropia();
      }
      else
      {
        cuenta = PedirCuenta("Numero de cuenta");
        if (cuenta is null)
          return;
      }

      decimal? importe = PedirImporte();
      if (importe is null)
        return;

      if (!ConfigHTTP.EsCliente())
      {
        string? leida = PedirClaveCuenta("Clave de la cuenta");
        if (leida is null)
          return;
        clave = leida;
      }

      if (!MessagesUtils.Confirm($"¿Retirar {Formato.Importe(importe, "USD")} de la cuenta {cuenta}?"))
      {
        MessagesUtils.PrintMessage("Operacion cancelada");
        return;
      }

      MessagesUtils.PrintMessage("Procesando...");
      MovimientoResponseDTO response = await controller.RetiroAsync(cuenta, importe.Value, clave);

      PrintResponse(response);
    }

    //revisado y aprovado por Marcos Escobar
    private static async Task Transferencia()
    {
      MessagesUtils.PrintTitle("TRANSFERENCIA");

      string? cuentaOrigen;
      string clave = "";

      if (ConfigHTTP.EsCliente())
      {
        cuentaOrigen = ConfigHTTP.Cuenta;
        await MostrarCuentaPropia();
      }
      else
      {
        cuentaOrigen = PedirCuenta("Cuenta de origen");
        if (cuentaOrigen is null)
          return;
      }

      string? cuentaDestino = PedirCuenta("Cuenta de destino");
      if (cuentaDestino is null)
        return;

      if (cuentaDestino == cuentaOrigen)
      {
        MessagesUtils.PrintError(ValidadorDatos.CuentasIguales);
        return;
      }

      decimal? importe = PedirImporte();
      if (importe is null)
        return;

      if (!ConfigHTTP.EsCliente())
      {
        string? leida = PedirClaveCuenta("Clave de la cuenta de origen");
        if (leida is null)
          return;
        clave = leida;
      }

      if (!MessagesUtils.Confirm(
            $"¿Transferir {Formato.Importe(importe, "USD")} de la cuenta {cuentaOrigen} a la cuenta {cuentaDestino}?"))
      {
        MessagesUtils.PrintMessage("Operacion cancelada");
        return;
      }

      MessagesUtils.PrintMessage("Procesando...");
      MovimientoResponseDTO response = await controller.TransferenciaAsync(
        cuentaOrigen,
        cuentaDestino,
        importe.Value,
        clave
      );

      PrintResponse(response);
    }

    //revisado y aprovado por Marcos Escobar
    // RNF-010: al terminar se muestra el número de movimiento y el nuevo saldo, o el mensaje del servidor.
    private static void PrintResponse(MovimientoResponseDTO response)
    {
      MessagesUtils.PrintTitle("RESULTADO");

      if (MenuView.RevisarSesion(response))
        return;

      if (response.GetEstado() != EstadosEnum.OK)
      {
        MessagesUtils.PrintError(response.GetMensaje());
        return;
      }

      MessagesUtils.PrintMessage(response.GetMensaje());
      MessagesUtils.PrintMessage("N.º de movimiento: " + response.NumeroMovimiento);
      MessagesUtils.PrintMessage("Nuevo saldo:       " + Formato.Importe(response.NuevoSaldo, "USD"));
      MessagesUtils.PrintMessage("Fecha:             " + Formato.Fecha(DateTime.Now));
    }

    //revisado y aprovado por Marcos Escobar
    // ECUD3 paso 2: el cliente ve el número y el saldo de su cuenta.
    private static async Task MostrarCuentaPropia()
    {
      ConsultaResponseDTO cuenta = await controller.ConsultarAsync(ConfigHTTP.Cuenta);

      string saldo = cuenta.GetEstado() == EstadosEnum.OK
        ? Formato.Importe(cuenta.Saldo, cuenta.Moneda)
        : Formato.SinDato;

      MessagesUtils.PrintMessage($"Cuenta: {ConfigHTTP.Cuenta}  Saldo: {saldo}");
    }

    //revisado y aprovado por Marcos Escobar
    private static string? PedirCuenta(string etiqueta)
    {
      string cuenta = MessagesUtils.ReadText(etiqueta);
      string? error = ValidadorDatos.ErrorCuenta(cuenta);

      if (error is null)
        return cuenta;

      MessagesUtils.PrintError(error);
      return null;
    }

    //revisado y aprovado por Marcos Escobar
    private static decimal? PedirImporte()
    {
      string texto = MessagesUtils.ReadText("Importe");
      string? error = ValidadorDatos.ErrorImporte(texto);

      if (error is null)
        return ValidadorDatos.ConvertirImporte(texto);

      MessagesUtils.PrintError(error);
      return null;
    }

    //revisado y aprovado por Marcos Escobar
    private static string? PedirClaveCuenta(string etiqueta)
    {
      string clave = MessagesUtils.ReadPassword(etiqueta);
      string? error = ValidadorDatos.ErrorClaveCuenta(clave);

      if (error is null)
        return clave;

      MessagesUtils.PrintError(error);
      return null;
    }
  }
}
