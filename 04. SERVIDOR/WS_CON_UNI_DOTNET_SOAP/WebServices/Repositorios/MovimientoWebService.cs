using WS_CON_UNI_DOTNET_SOAP.Controllers;
using WS_CON_UNI_DOTNET_SOAP.Models.Data;
using WS_CON_UNI_DOTNET_SOAP.Models.DTO;
using WS_CON_UNI_DOTNET_SOAP.Models.Enums;
using WS_CON_UNI_DOTNET_SOAP.Services;
using WS_CON_UNI_DOTNET_SOAP.WS.Interfaces;
//revisado y aprovado por Marcos Escobar
using Microsoft.Data.SqlClient;

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
        //revisado y aprovado por Marcos Escobar
        MovimientoController.Autorizar(TokenService.GetSesion(token), "DEPOSITO", cuentaOrigen);

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
        //revisado y aprovado por Marcos Escobar
        MovimientoController.Autorizar(TokenService.GetSesion(token), "RETIRO", cuentaOrigen);

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
        //revisado y aprovado por Marcos Escobar
        MovimientoController.Autorizar(TokenService.GetSesion(token), "TRANSFERENCIA", cuentaOrigen);

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

    //revisado y aprovado por Marcos Escobar
    public ConsultaMovimientosResponseDTO ConsultarMovimientos(string token, string cuenta)
    {
      Sesion? sesion = TokenService.GetSesion(token);
      if (sesion == null)
        return new ConsultaMovimientosResponseDTO
        {
          Estado = EstadosEnum.ERROR,
          Mensaje = MovimientoController.TOKEN_INVALIDO
        };

      try
      {
        Cuenta datos = MovimientoController.GetCuentaConsultable(sesion, cuenta);
        Usuario titular = LoginController.GetUsuarioById(cuenta);

        return new ConsultaMovimientosResponseDTO
        {
          Estado = EstadosEnum.OK,
          Mensaje = "Consulta de movimientos exitosa",
          Cuenta = new CuentaDTO
          {
            NumeroCuenta = datos.Id,
            Titular = titular.Nombre,
            Saldo = MovimientoDTO.Monto(datos.Saldo),
            EstadoCuenta = datos.Estado
          },
          Movimientos = MovimientoController.GetMovimientosByUserId(cuenta)
            .Select(m => MovimientoDTO.Desde(m, cuenta))
            .ToList()
        };
      }
      catch (SqlException)
      {
        return new ConsultaMovimientosResponseDTO
        {
          Estado = EstadosEnum.ERROR,
          Mensaje = LoginController.SERVICIO_NO_DISPONIBLE
        };
      }
      catch (Exception e)
      {
        return new ConsultaMovimientosResponseDTO { Estado = EstadosEnum.ERROR, Mensaje = e.Message };
      }
    }

    //revisado y aprovado por Marcos Escobar
    public MovimientoResponseDTO RegistrarDeposito(string token, string cuenta, decimal importe)
    {
      return Registrar(token, "DEPOSITO", cuenta, null, importe, null, "Depósito registrado correctamente");
    }

    //revisado y aprovado por Marcos Escobar
    public MovimientoResponseDTO RegistrarRetiro(string token, string cuenta, decimal importe, string clave)
    {
      return Registrar(token, "RETIRO", cuenta, null, importe, clave, "Retiro registrado correctamente");
    }

    //revisado y aprovado por Marcos Escobar
    public MovimientoResponseDTO RegistrarTransferencia(
      string token,
      string cuentaOrigen,
      string cuentaDestino,
      decimal importe,
      string clave
    )
    {
      return Registrar(
        token, "TRANSFERENCIA", cuentaOrigen, cuentaDestino, importe, clave,
        "Transferencia registrada correctamente"
      );
    }

    //revisado y aprovado por Marcos Escobar
    private static MovimientoResponseDTO Registrar(
      string token,
      string tipo,
      string? cuentaOrigen,
      string? cuentaDestino,
      decimal importe,
      string? clave,
      string mensajeOk
    )
    {
      try
      {
        Movimiento movimiento = MovimientoController.Registrar(
          TokenService.GetSesion(token), tipo, cuentaOrigen, cuentaDestino, importe, clave
        );
        int numero = int.Parse(movimiento.Id);

        return new MovimientoResponseDTO
        {
          Estado = EstadosEnum.OK,
          Mensaje = mensajeOk,
          IdMovimiento = numero,
          NumeroMovimiento = numero,
          NuevoSaldo = MovimientoDTO.Monto(movimiento.NuevoSaldo)
        };
      }
      catch (Exception e)
      {
        return new MovimientoResponseDTO { Estado = EstadosEnum.ERROR, Mensaje = e.Message };
      }
    }
  }
}
