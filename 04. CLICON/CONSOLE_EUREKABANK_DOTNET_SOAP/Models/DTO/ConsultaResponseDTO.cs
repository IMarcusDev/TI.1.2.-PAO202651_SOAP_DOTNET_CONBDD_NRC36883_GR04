using CONSOLE_EUREKABANK_DOTNET_SOAP.Models.Enums;
using System.Xml.Linq;

namespace CONSOLE_EUREKABANK_DOTNET_SOAP.Models.DTO
{
  /// <summary>Respuesta de consultarMovimientos: los datos de la cuenta y la lista de sus movimientos.</summary>
  //revisado y aprovado por Marcos Escobar
  internal class ConsultaResponseDTO : ResponseDTO
  {
    public string NumeroCuenta { get; set; } = "";
    public string Titular { get; set; } = "";
    public string Moneda { get; set; } = "";
    public decimal? Saldo { get; set; }
    public string EstadoCuenta { get; set; } = "";
    public List<MovimientoDTO> Movimientos { get; set; } = new List<MovimientoDTO>();

    public ConsultaResponseDTO(EstadosEnum estado, string mensaje) : base(estado, mensaje)
    {
    }

    /// <summary>
    /// Los movimientos pueden venir envueltos (&lt;Movimientos&gt;&lt;MovimientoDTO&gt;…, XmlSerializer) o como
    /// elementos repetidos: se toma cada elemento que tenga numeroMovimiento y accion.
    /// </summary>
    public static new ConsultaResponseDTO FromXML(string xml)
    {
      ResponseDTO response = ResponseDTO.FromXML(xml);
      XElement cuerpo = Cuerpo(xml);

      ConsultaResponseDTO dto = new ConsultaResponseDTO(response.GetEstado(), response.GetMensaje())
      {
        NumeroCuenta = LeerTexto(cuerpo, "numeroCuenta"),
        Titular = LeerTexto(cuerpo, "titular"),
        Moneda = LeerTexto(cuerpo, "moneda"),
        Saldo = LeerDecimal(cuerpo, "saldo"),
        EstadoCuenta = LeerTexto(cuerpo, "estadoCuenta")
      };

      foreach (XElement m in cuerpo.Descendants().Where(EsMovimiento))
      {
        dto.Movimientos.Add(new MovimientoDTO
        {
          NumeroMovimiento = LeerEntero(m, "numeroMovimiento"),
          Fecha = LeerTexto(m, "fecha"),
          TipoMovimiento = LeerTexto(m, "tipoMovimiento"),
          Accion = LeerTexto(m, "accion"),
          Importe = LeerDecimal(m, "importe"),
          CuentaReferencia = LeerTexto(m, "cuentaReferencia")
        });
      }

      return dto;
    }

    private static bool EsMovimiento(XElement e)
    {
      bool Tiene(string nombre) => e.Elements().Any(h => h.Name.LocalName.Equals(nombre, StringComparison.OrdinalIgnoreCase));
      return Tiene("numeroMovimiento") && Tiene("accion");
    }
  }

  /// <summary>Un movimiento de la cuenta consultada.</summary>
  //revisado y aprovado por Marcos Escobar
  internal class MovimientoDTO
  {
    public int NumeroMovimiento { get; set; }
    public string Fecha { get; set; } = "";
    public string TipoMovimiento { get; set; } = "";
    public string Accion { get; set; } = "";
    public decimal? Importe { get; set; }
    public string CuentaReferencia { get; set; } = "";

    public bool EsIngreso()
    {
      return Accion.Equals("INGRESO", StringComparison.OrdinalIgnoreCase);
    }
  }
}
