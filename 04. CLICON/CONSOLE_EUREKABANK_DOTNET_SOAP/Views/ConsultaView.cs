using CONSOLE_EUREKABANK_DOTNET_SOAP.Models.DTO;
using CONSOLE_EUREKABANK_DOTNET_SOAP.Models.Enums;
using CONSOLE_EUREKABANK_DOTNET_SOAP.Controllers;
using CONSOLE_EUREKABANK_DOTNET_SOAP.Models.HTTP;
using CONSOLE_EUREKABANK_DOTNET_SOAP.Views.Utils;

namespace CONSOLE_EUREKABANK_DOTNET_SOAP.Views
{
  /// <summary>ECUD2: cabecera de la cuenta y tabla con N.º Mov., Fecha, Tipo, Acción, Importe y Cuenta de referencia.</summary>
  //revisado y aprovado por Marcos Escobar
  internal class ConsultaView
  {
    private static MovimientoController controller = new MovimientoController();

    public static async Task Show()
    {
      MessagesUtils.PrintTitle("CONSULTAR MOVIMIENTOS");

      // ECUD2 paso 2: el cliente consulta la cuenta de su sesión.
      string cuenta = ConfigHTTP.Cuenta;

      if (!ConfigHTTP.EsCliente())
      {
        cuenta = MessagesUtils.ReadText("Numero de cuenta");
        string? error = ValidadorDatos.ErrorCuenta(cuenta);
        if (error is not null)
        {
          MessagesUtils.PrintError(error);
          return;
        }
      }

      MessagesUtils.PrintMessage("Procesando...");
      ConsultaResponseDTO res = await controller.ConsultarAsync(cuenta);

      if (MenuView.RevisarSesion(res))
        return;

      if (res.GetEstado() != EstadosEnum.OK)
      {
        MessagesUtils.PrintError(res.GetMensaje());
        return;
      }

      Console.WriteLine();
      Console.WriteLine($"Cuenta:  {res.NumeroCuenta}");
      Console.WriteLine($"Titular: {res.Titular}");
      Console.WriteLine($"Moneda:  {res.Moneda}");
      Console.WriteLine($"Saldo:   {Formato.Importe(res.Saldo, res.Moneda)}");
      Console.WriteLine($"Estado:  {res.EstadoCuenta}");
      Console.WriteLine();

      if (res.Movimientos.Count == 0)
      {
        MessagesUtils.PrintMessage("La cuenta no registra movimientos");
        return;
      }

      List<string[]> filas = res.Movimientos
        .Select(m => new[]
        {
          m.NumeroMovimiento.ToString(),
          Formato.Fecha(m.Fecha),
          m.TipoMovimiento,
          m.Accion,
          (m.EsIngreso() ? "+" : "-") + Formato.Importe(m.Importe),
          string.IsNullOrWhiteSpace(m.CuentaReferencia) ? Formato.SinDato : m.CuentaReferencia
        })
        .ToList();

      MessagesUtils.PrintTable(
        new[] { "N.º Mov.", "Fecha", "Tipo", "Acción", "Importe", "Cuenta ref." },
        filas,
        new[] { true, false, false, false, true, false }
      );
    }
  }
}
