using System.Globalization;
using WS_CON_UNI_DOTNET_SOAP.Models.Data;

namespace WS_CON_UNI_DOTNET_SOAP.Models.DTO
{
  /// <summary>Un movimiento visto desde una cuenta, como lo devuelve consultarMovimientos.</summary>
  //revisado y aprovado por Marcos Escobar
  public class MovimientoDTO
  {
    public int NumeroMovimiento { get; set; }
    public string Fecha { get; set; } = "";
    public string TipoMovimiento { get; set; } = "";
    public string Accion { get; set; } = "";
    public decimal Importe { get; set; }
    public string CuentaReferencia { get; set; } = "";

    /// <summary>
    /// La base guarda una transferencia en una sola fila (origen → destino). Para la
    /// cuenta de origen es una SALIDA y para la de destino un INGRESO; la cuenta de
    /// referencia es la otra cuenta. Un depósito es INGRESO y un retiro, SALIDA.
    /// </summary>
    public static MovimientoDTO Desde(Movimiento m, string cuenta)
    {
      MovimientoDTO dto = new MovimientoDTO
      {
        NumeroMovimiento = int.Parse(m.Id),
        Fecha = m.Fecha?.ToString("yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture) ?? "",
        Importe = Monto(m.Valor)
      };

      switch (m.Tipo)
      {
        case "DEPOSITO":
          dto.TipoMovimiento = "Depósito";
          dto.Accion = "INGRESO";
          break;
        case "RETIRO":
          dto.TipoMovimiento = "Retiro";
          dto.Accion = "SALIDA";
          break;
        default:
          bool salida = cuenta == m.CuentaOrigen;
          dto.TipoMovimiento = "Transferencia";
          dto.Accion = salida ? "SALIDA" : "INGRESO";
          dto.CuentaReferencia = salida ? m.CuentaDestino : m.CuentaOrigen;
          break;
      }

      return dto;
    }

    /// <summary>Importe con dos decimales.</summary>
    public static decimal Monto(double valor)
    {
      return Math.Round((decimal)valor, 2, MidpointRounding.AwayFromZero);
    }
  }
}
