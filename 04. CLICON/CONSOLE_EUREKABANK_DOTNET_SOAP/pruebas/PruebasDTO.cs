//revisado y aprovado por Marcos Escobar
using CONSOLE_EUREKABANK_DOTNET_SOAP.Models.DTO;
using CONSOLE_EUREKABANK_DOTNET_SOAP.Models.Enums;

namespace CONSOLE_EUREKABANK_DOTNET_SOAP.Pruebas;

internal static class PruebasDTO
{
    private static string Sobre(string respuesta) =>
        "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\"><s:Body>" + respuesta + "</s:Body></s:Envelope>";

    public static Ejecutor Crear()
    {
        var e = new Ejecutor();

        e.Caso("estado y mensaje en PascalCase (SoapCore)", () =>
        {
            var r = ResponseDTO.FromXML(Sobre("<logoutResponse xmlns=\"http://tempuri.org/\"><logoutResult><Estado>OK</Estado><Mensaje>Sesión cerrada</Mensaje></logoutResult></logoutResponse>"));
            Afirmar.Igual(EstadosEnum.OK, r.GetEstado());
            Afirmar.Igual("Sesión cerrada", r.GetMensaje());
        });

        e.Caso("Token inválido se reconoce como sesión expirada", () =>
        {
            var r = ResponseDTO.FromXML(Sobre("<x><Estado>ERROR</Estado><Mensaje>Token inválido o sesión expirada</Mensaje></x>"));
            Afirmar.Verdadero(r.SesionExpirada(), "token");
        });

        e.Caso("Un SOAP Fault se convierte en SoapFallaException", () =>
        {
            try
            {
                ResponseDTO.FromXML(Sobre("<s:Fault><faultcode>s:Client</faultcode><faultstring>No existe</faultstring></s:Fault>"));
                Afirmar.Verdadero(false, "debía lanzar");
            }
            catch (SoapFallaException)
            {
            }
        });

        e.Caso("Login trae token, perfil y nombre", () =>
        {
            var dto = LoginResponseDTO.FromXML(Sobre("<loginEmpleadoResponse xmlns=\"http://tempuri.org/\"><loginEmpleadoResult><Estado>OK</Estado><Mensaje>Ingreso de sesión exitoso</Mensaje><Token>t-1</Token><Nombre>Carla Andrade</Nombre><EstadoUsuario>ACTIVO</EstadoUsuario><TipoUsuario>Empleado</TipoUsuario><Perfil>EMPLEADO</Perfil></loginEmpleadoResult></loginEmpleadoResponse>"));
            Afirmar.Igual(EstadosEnum.OK, dto.GetEstado());
            Afirmar.Igual("t-1", dto.GetToken());
            Afirmar.Igual("EMPLEADO", dto.perfil);
            Afirmar.Igual("Carla Andrade", dto.nombre);
        });

        e.Caso("Operación trae número de movimiento y nuevo saldo", () =>
        {
            var dto = MovimientoResponseDTO.FromXML(Sobre("<r><Estado>OK</Estado><Mensaje>ok</Mensaje><IdMovimiento>23</IdMovimiento><NumeroMovimiento>23</NumeroMovimiento><NuevoSaldo>475.00</NuevoSaldo></r>"));
            Afirmar.Igual(23, dto.NumeroMovimiento);
            Afirmar.Igual(475.00m, dto.NuevoSaldo);
        });

        e.Caso("Consulta: cuenta y movimientos envueltos (XmlSerializer)", () =>
        {
            var dto = ConsultaResponseDTO.FromXML(Sobre(
                "<consultarMovimientosResponse xmlns=\"http://tempuri.org/\"><consultarMovimientosResult>"
                + "<Estado>OK</Estado><Mensaje>Consulta de movimientos exitosa</Mensaje>"
                + "<Cuenta><NumeroCuenta>00100001</NumeroCuenta><Titular>Ana Torres</Titular><Moneda>USD</Moneda><Saldo>575.00</Saldo><EstadoCuenta>ACTIVO</EstadoCuenta></Cuenta>"
                + "<Movimientos>"
                + "<MovimientoDTO><NumeroMovimiento>23</NumeroMovimiento><Fecha>2026-11-03T16:07:05</Fecha><TipoMovimiento>Transferencia</TipoMovimiento><Accion>SALIDA</Accion><Importe>25.00</Importe><CuentaReferencia>00100002</CuentaReferencia></MovimientoDTO>"
                + "<MovimientoDTO><NumeroMovimiento>24</NumeroMovimiento><Fecha>2026-11-03T16:09:00</Fecha><TipoMovimiento>Depósito</TipoMovimiento><Accion>INGRESO</Accion><Importe>100.00</Importe><CuentaReferencia /></MovimientoDTO>"
                + "</Movimientos></consultarMovimientosResult></consultarMovimientosResponse>"));
            Afirmar.Igual(EstadosEnum.OK, dto.GetEstado());
            Afirmar.Igual("00100001", dto.NumeroCuenta);
            Afirmar.Igual(575.00m, dto.Saldo);
            Afirmar.Igual("ACTIVO", dto.EstadoCuenta);
            Afirmar.Igual(2, dto.Movimientos.Count);
            Afirmar.Igual("00100002", dto.Movimientos[0].CuentaReferencia);
            Afirmar.Verdadero(dto.Movimientos[1].EsIngreso(), "ingreso");
        });

        e.Caso("Consulta sin movimientos y error sin datos", () =>
        {
            var vacia = ConsultaResponseDTO.FromXML(Sobre("<r><Estado>OK</Estado><Mensaje>ok</Mensaje><Cuenta><NumeroCuenta>00100002</NumeroCuenta></Cuenta><Movimientos /></r>"));
            Afirmar.Igual(0, vacia.Movimientos.Count);
            var error = ConsultaResponseDTO.FromXML(Sobre("<r><Estado>ERROR</Estado><Mensaje>La cuenta no existe</Mensaje></r>"));
            Afirmar.Igual("La cuenta no existe", error.GetMensaje());
        });

        //revisado y aprovado por Marcos Escobar
        e.Caso("Consulta: la tabla muestra primero el último movimiento", () =>
        {
            var dto = new ConsultaResponseDTO(EstadosEnum.OK, "ok");
            foreach (int n in new[] { 3, 6, 10, 9 })
                dto.Movimientos.Add(new MovimientoDTO { NumeroMovimiento = n });
            Afirmar.Igual("10,9,6,3", string.Join(",", dto.MovimientosRecientes().Select(m => m.NumeroMovimiento)));
            Afirmar.Igual("3,6,10,9", string.Join(",", dto.Movimientos.Select(m => m.NumeroMovimiento)));
        });

        return e;
    }
}
