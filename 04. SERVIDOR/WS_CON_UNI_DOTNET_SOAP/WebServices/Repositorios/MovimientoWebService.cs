using WS_CON_UNI_DOTNET_SOAP.Controllers;
using WS_CON_UNI_DOTNET_SOAP.Models.Data;
using WS_CON_UNI_DOTNET_SOAP.Models.DTO;
using WS_CON_UNI_DOTNET_SOAP.Models.Enums;
using WS_CON_UNI_DOTNET_SOAP.Services;
using WS_CON_UNI_DOTNET_SOAP.WS.Interfaces;

namespace WS_CON_UNI_DOTNET_SOAP.WS.Services
{
  public class MovimientoWebService : IMovimientoWebService
  {
    public MovimientoResponseDTO Deposito(
      string token,
      string cuentaOrigen,
      string cuentaDestino,
      double valor
    )
    {
      if (!TokenService.ValidateToken(token))
        return new MovimientoResponseDTO
        {
          Estado = EstadosEnum.ERROR,
          Mensaje = "Token inválido"
        };

      try
      {
        Movimiento movimiento = MovimientoController.RegisterDeposito(
          new Movimiento {
            Tipo = "DEPOSITO",
            CuentaOrigen = cuentaOrigen,
            CuentaDestino = cuentaDestino,
            Valor = valor
          }
        );

        return new MovimientoResponseDTO {
          Estado = EstadosEnum.OK,
          Mensaje = "Depósito registrado exitosamente",
          IdMovimiento = int.Parse(movimiento.Id)
        };
      }
      catch (Exception e)
      {
        return new MovimientoResponseDTO
        {
          Estado = EstadosEnum.ERROR,
          Mensaje = e.Message,
        };
      }
    }

    public MovimientoResponseDTO Retiro(
      string token,
      string cuentaOrigen,
      string cuentaDestino,
      double valor
    )
    {
      if (!TokenService.ValidateToken(token))
        return new MovimientoResponseDTO
        {
          Estado = EstadosEnum.ERROR,
          Mensaje = "Token inválido"
        };

      try
      {
        Movimiento movimiento = MovimientoController.RegisterRetiro(
          new Movimiento {
            Tipo = "RETIRO",
            CuentaOrigen = cuentaOrigen,
            CuentaDestino = cuentaDestino,
            Valor = valor
          }
        );

        return new MovimientoResponseDTO
        {
          Estado = EstadosEnum.OK,
          Mensaje = "Retiro registrado exitosamente",
          IdMovimiento = int.Parse(movimiento.Id)
        };
      }
      catch (Exception e)
      {
        return new MovimientoResponseDTO {
          Estado = EstadosEnum.ERROR,
          Mensaje = e.Message
        };
      }
    }

    public MovimientoResponseDTO Transferencia(
      string token,
      string cuentaOrigen,
      string cuentaDestino,
      double valor
    )
    {
      if (!TokenService.ValidateToken(token))
        return new MovimientoResponseDTO
        {
          Estado = EstadosEnum.ERROR,
          Mensaje = "Token inválido"
        };

      try
      {
        Movimiento movimiento = MovimientoController.RegisterTransferencia(
          new Movimiento {
            Tipo = "TRANSFERENCIA",
            CuentaOrigen = cuentaOrigen,
            CuentaDestino = cuentaDestino,
            Valor = valor
          }
        );

        return new MovimientoResponseDTO
        {
          Estado = EstadosEnum.OK,
          Mensaje = "Transferencia registrada exitosamente",
          IdMovimiento = int.Parse(movimiento.Id)
        };
      }
      catch (Exception e)
      {
        return new MovimientoResponseDTO
        {
          Estado = EstadosEnum.ERROR,
          Mensaje = e.Message,
        };
      }
    }
  }
}