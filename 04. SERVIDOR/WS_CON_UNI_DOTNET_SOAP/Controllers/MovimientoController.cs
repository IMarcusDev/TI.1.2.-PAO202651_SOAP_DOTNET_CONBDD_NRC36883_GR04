using WS_CON_UNI_DOTNET_SOAP.Models.Data;
using WS_CON_UNI_DOTNET_SOAP.Models.Interfaces;
using WS_CON_UNI_DOTNET_SOAP.Models.Repositories;
using System.Collections.Generic;

namespace WS_CON_UNI_DOTNET_SOAP.Controllers
{
  public class MovimientoController
  {
    private static readonly IMovimientoRepository repository = new MovimientoImplRepository();
    private static readonly ICuentaRepository cuentaRepo = new CuentaImplRepository();
    private static readonly ILoginRepository loginRepo = new LoginImplRepository();

    public static List<Movimiento> GetMovimientosByUserId(string id)
    {
      return MovimientoController.repository.GetMovimientosByUserId(id);
    }

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

      // Cuenta existente
      Cuenta destino = cuentaRepo.GetCuenta(m.CuentaDestino);
      destino.Saldo += m.Valor;

      cuentaRepo.UpdateCuenta(destino);

      return MovimientoController.repository.RegisterMovimiento(m);
    }

    public static Movimiento RegisterRetiro(Movimiento m)
    {
      if (!m.Tipo.Equals("RETIRO"))
        throw new Exception("El movimiento no es retiro");

      if (m.Valor <= 0)
        throw new Exception("El valor debe ser mayor a cero");

      // Cuenta existente
      Cuenta origen = cuentaRepo.GetCuenta(m.CuentaOrigen);

      // Cuenta con plata
      if (m.Valor > origen.Saldo)
        throw new Exception("Saldo insuficiente");

      origen.Saldo -= m.Valor;
      cuentaRepo.UpdateCuenta(origen);

      return MovimientoController.repository.RegisterMovimiento(m);
    }

    public static Movimiento RegisterTransferencia(Movimiento m)
    {
      if (!m.Tipo.Equals("TRANSFERENCIA"))
        throw new Exception("El movimiento no es transferencia");

      if (m.Valor <= 0)
        throw new Exception("El valor debe ser mayor a cero");

      // Cuentas existentes
      Cuenta origen = cuentaRepo.GetCuenta(m.CuentaOrigen);
      Cuenta destino = cuentaRepo.GetCuenta(m.CuentaDestino);

      // Cuenta origen con plata
      if (m.Valor > origen.Saldo)
        throw new Exception("Saldo insuficiente");

      origen.Saldo -= m.Valor;
      cuentaRepo.UpdateCuenta(origen);

      destino.Saldo += m.Valor;
      cuentaRepo.UpdateCuenta(destino);

      return MovimientoController.repository.RegisterMovimiento(m);
    }
  }
}