using CONSOLE_EUREKABANK_DOTNET_SOAP.Models.Enums;
using System.Xml.Linq;

namespace CONSOLE_EUREKABANK_DOTNET_SOAP.Models.DTO
{
  internal class MovimientoResponseDTO : ResponseDTO
  {
    public int IdMovimiento { get; set; }
    //revisado y aprovado por Marcos Escobar
    // Datos del comprobante (RNF-010): número de movimiento y nuevo saldo.
    public int NumeroMovimiento { get; set; }
    public decimal? NuevoSaldo { get; set; }

    public MovimientoResponseDTO(EstadosEnum estado, string mensaje, int idMovimiento) : base(estado, mensaje)
    {
      this.IdMovimiento = idMovimiento;
    }

    //revisado y aprovado por Marcos Escobar
    public static new MovimientoResponseDTO FromXML(string xml)
    {
      ResponseDTO response = ResponseDTO.FromXML(xml);
      XElement cuerpo = Cuerpo(xml);

      return new MovimientoResponseDTO(
        response.GetEstado(),
        response.GetMensaje(),
        LeerEntero(cuerpo, "idMovimiento")
      )
      {
        NumeroMovimiento = LeerEntero(cuerpo, "numeroMovimiento"),
        NuevoSaldo = LeerDecimal(cuerpo, "nuevoSaldo")
      };
    }

  }
}
