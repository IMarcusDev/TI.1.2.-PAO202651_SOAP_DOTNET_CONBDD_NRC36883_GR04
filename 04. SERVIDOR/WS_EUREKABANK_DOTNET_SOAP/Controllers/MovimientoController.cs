using WS_CON_UNI_DOTNET_SOAP.Models.Data;
using WS_CON_UNI_DOTNET_SOAP.Models.Interfaces;
using WS_CON_UNI_DOTNET_SOAP.Models.Repositories;
using System.Collections.Generic;
//revisado y aprovado por Marcos Escobar
using Microsoft.Data.SqlClient;
using WS_CON_UNI_DOTNET_SOAP.Config;
using WS_CON_UNI_DOTNET_SOAP.Services;

namespace WS_CON_UNI_DOTNET_SOAP.Controllers
{
  public class MovimientoController
  {
    private static readonly IMovimientoRepository repository = new MovimientoImplRepository();
    private static readonly ICuentaRepository cuentaRepo = new CuentaImplRepository();
    private static readonly ILoginRepository loginRepo = new LoginImplRepository();

    //revisado y aprovado por Marcos Escobar
    public const string TOKEN_INVALIDO = "Token inválido o sesión expirada";
    public const string NO_PERMITIDA = "Operación no permitida para su perfil";
    public const string CUENTA_NO_EXISTE = "La cuenta no existe";
    public const string CUENTA_INACTIVA = "La cuenta no se encuentra activa";
    public const string DESTINO_NO_EXISTE = "La cuenta destino no existe";
    public const string DESTINO_INACTIVO = "La cuenta destino no se encuentra activa";
    public const string CUENTAS_IGUALES = "La cuenta de origen y la de destino deben ser diferentes";
    public const string CLAVE_INCORRECTA = "Clave de cuenta incorrecta";
    public const string IMPORTE_INVALIDO = "El importe debe ser mayor que cero y tener como máximo dos decimales";
    public const string ERROR_REGISTRO = "No se pudo registrar la operación, intente nuevamente";

    public static List<Movimiento> GetMovimientosByUserId(string id)
    {
      return MovimientoController.repository.GetMovimientosByUserId(id);
    }

    //revisado y aprovado por Marcos Escobar
    // Las validaciones originales se mantienen; la actualización del saldo y el registro
    // del movimiento ahora van en una sola transacción con la fila de la cuenta bloqueada.
    public static Movimiento RegisterDeposito(Movimiento m)
    {
      if (!m.Tipo.Equals("DEPOSITO"))
        throw new Exception("El movimiento no es depósito");
      if (m.Valor <= 0)
        throw new Exception("El valor debe ser mayor a cero");

      // Perfil de Empleado
      Usuario empleado = loginRepo.GetUsuario(m.CuentaOrigen);
      if (!empleado.Tipo.Equals("Empleado"))
        throw new Exception("Usuario no es empleado");

      return EnTransaccion((connection, transaction) =>
      {
        // Cuenta existente y activa
        Cuenta destino = Bloquear(connection, transaction, m.CuentaDestino, CUENTA_NO_EXISTE, CUENTA_INACTIVA);
        destino.Saldo += m.Valor;

        cuentaRepo.UpdateCuenta(connection, transaction, destino);

        Movimiento registrado = MovimientoController.repository.RegisterMovimiento(connection, transaction, m);
        registrado.NuevoSaldo = destino.Saldo;
        return registrado;
      });
    }

    //revisado y aprovado por Marcos Escobar
    public static Movimiento RegisterRetiro(Movimiento m)
    {
      if (!m.Tipo.Equals("RETIRO"))
        throw new Exception("El movimiento no es retiro");
      if (m.Valor <= 0)
        throw new Exception("El valor debe ser mayor a cero");

      // Un retiro no tiene cuenta destino (la columna tiene clave foránea hacia CUENTAS).
      m.CuentaDestino = "";

      return EnTransaccion((connection, transaction) =>
      {
        // Cuenta existente y activa
        Cuenta origen = Bloquear(connection, transaction, m.CuentaOrigen, CUENTA_NO_EXISTE, CUENTA_INACTIVA);

        // Cuenta con plata (se revisa con la fila bloqueada)
        if (m.Valor > origen.Saldo)
          throw new Exception("Saldo insuficiente");

        origen.Saldo -= m.Valor;
        cuentaRepo.UpdateCuenta(connection, transaction, origen);

        Movimiento registrado = MovimientoController.repository.RegisterMovimiento(connection, transaction, m);
        registrado.NuevoSaldo = origen.Saldo;
        return registrado;
      });
    }

    //revisado y aprovado por Marcos Escobar
    public static Movimiento RegisterTransferencia(Movimiento m)
    {
      if (!m.Tipo.Equals("TRANSFERENCIA"))
        throw new Exception("El movimiento no es transferencia");
      if (m.Valor <= 0)
        throw new Exception("El valor debe ser mayor a cero");
      if (string.IsNullOrWhiteSpace(m.CuentaDestino))
        throw new Exception(DESTINO_NO_EXISTE);
      if (m.CuentaDestino == m.CuentaOrigen)
        throw new Exception(CUENTAS_IGUALES);

      return EnTransaccion((connection, transaction) =>
      {
        // Cuentas existentes y activas. Se bloquean en orden de número para que dos
        // transferencias cruzadas no se esperen mutuamente.
        Cuenta origen;
        Cuenta destino;
        if (string.CompareOrdinal(m.CuentaOrigen, m.CuentaDestino) < 0)
        {
          origen = Bloquear(connection, transaction, m.CuentaOrigen, CUENTA_NO_EXISTE, CUENTA_INACTIVA);
          destino = Bloquear(connection, transaction, m.CuentaDestino, DESTINO_NO_EXISTE, DESTINO_INACTIVO);
        }
        else
        {
          destino = Bloquear(connection, transaction, m.CuentaDestino, DESTINO_NO_EXISTE, DESTINO_INACTIVO);
          origen = Bloquear(connection, transaction, m.CuentaOrigen, CUENTA_NO_EXISTE, CUENTA_INACTIVA);
        }

        // Cuenta origen con plata
        if (m.Valor > origen.Saldo)
          throw new Exception("Saldo insuficiente");

        origen.Saldo -= m.Valor;
        cuentaRepo.UpdateCuenta(connection, transaction, origen);

        destino.Saldo += m.Valor;
        cuentaRepo.UpdateCuenta(connection, transaction, destino);

        Movimiento registrado = MovimientoController.repository.RegisterMovimiento(connection, transaction, m);
        registrado.NuevoSaldo = origen.Saldo;
        return registrado;
      });
    }

    //revisado y aprovado por Marcos Escobar
    // Operaciones del ERS (registrarDeposito, registrarRetiro, registrarTransferencia):
    // revisa el perfil, el importe y la clave de la cuenta, y registra con los métodos de arriba.
    public static Movimiento Registrar(
      Sesion? sesion,
      string tipo,
      string? cuentaOrigen,
      string? cuentaDestino,
      decimal? importe,
      string? clave
    )
    {
      Autorizar(sesion, tipo, cuentaOrigen);
      ValidarImporte(importe);

      // El empleado opera con la clave de la cuenta que le entrega el cliente.
      if (!sesion!.EsCliente && tipo != "DEPOSITO")
        ValidarClaveCuenta(cuentaOrigen, clave);

      Movimiento m = new Movimiento
      {
        Tipo = tipo,
        CuentaOrigen = tipo == "DEPOSITO" ? sesion.Codigo : cuentaOrigen ?? "",
        CuentaDestino = tipo == "DEPOSITO" ? cuentaOrigen ?? "" : cuentaDestino ?? "",
        Valor = (double)importe!.Value
      };

      return tipo switch
      {
        "DEPOSITO" => RegisterDeposito(m),
        "RETIRO" => RegisterRetiro(m),
        _ => RegisterTransferencia(m)
      };
    }

    //revisado y aprovado por Marcos Escobar
    // El depósito solo lo hace un empleado; el cliente solo opera sobre la cuenta de su sesión.
    public static void Autorizar(Sesion? sesion, string tipo, string? cuentaOrigen)
    {
      if (sesion == null)
        throw new Exception(TOKEN_INVALIDO);
      if (sesion.EsCliente && (tipo == "DEPOSITO" || sesion.Codigo != cuentaOrigen))
        throw new Exception(NO_PERMITIDA);
    }

    //revisado y aprovado por Marcos Escobar
    // Consulta: el empleado ve cualquier cuenta y el cliente solo la suya. No exige cuenta activa.
    public static Cuenta GetCuentaConsultable(Sesion sesion, string? cuenta)
    {
      if (sesion.EsCliente && sesion.Codigo != cuenta)
        throw new Exception(NO_PERMITIDA);

      try
      {
        return cuentaRepo.GetCuenta(cuenta ?? "");
      }
      catch (SqlException)
      {
        throw new Exception(LoginController.SERVICIO_NO_DISPONIBLE);
      }
      catch (Exception)
      {
        throw new Exception(CUENTA_NO_EXISTE);
      }
    }

    //revisado y aprovado por Marcos Escobar
    public static void ValidarImporte(decimal? importe)
    {
      if (importe == null
          || importe <= 0
          || decimal.Round(importe.Value, 2) != importe.Value
          || decimal.Truncate(importe.Value) >= 10_000_000_000m)
        throw new Exception(IMPORTE_INVALIDO);
    }

    //revisado y aprovado por Marcos Escobar
    // La clave de la cuenta es la del perfil del titular (mismo ID que la cuenta).
    public static void ValidarClaveCuenta(string? cuenta, string? clave)
    {
      Perfil perfil;

      try
      {
        perfil = loginRepo.GetPerfilById(cuenta ?? "");
      }
      catch (SqlException)
      {
        throw new Exception(LoginController.SERVICIO_NO_DISPONIBLE);
      }
      catch (Exception)
      {
        throw new Exception(CUENTA_NO_EXISTE);
      }

      if (clave == null || !HashService.VerifyPassword(clave, perfil.PasswordHash))
        throw new Exception(CLAVE_INCORRECTA);
    }

    //revisado y aprovado por Marcos Escobar
    // Todo o nada: si algo falla se deshacen el saldo y el movimiento. Los errores de
    // base de datos no se muestran al usuario tal cual.
    private static Movimiento EnTransaccion(Func<SqlConnection, SqlTransaction, Movimiento> operacion)
    {
      try
      {
        using (SqlConnection connection = DBConfig.GetConnection())
        {
          connection.Open();

          using (SqlTransaction transaction = connection.BeginTransaction())
          {
            try
            {
              Movimiento registrado = operacion(connection, transaction);
              transaction.Commit();
              return registrado;
            }
            catch
            {
              try { transaction.Rollback(); } catch (Exception) { }
              throw;
            }
          }
        }
      }
      catch (SqlException)
      {
        throw new Exception(ERROR_REGISTRO);
      }
    }

    //revisado y aprovado por Marcos Escobar
    private static Cuenta Bloquear(
      SqlConnection connection,
      SqlTransaction transaction,
      string? id,
      string noExiste,
      string inactiva
    )
    {
      Cuenta? cuenta = string.IsNullOrEmpty(id) ? null : cuentaRepo.GetCuentaParaActualizar(connection, transaction, id);

      if (cuenta == null)
        throw new Exception(noExiste);
      if (!LoginController.EsActivo(cuenta.Estado))
        throw new Exception(inactiva);

      return cuenta;
    }
  }
}
