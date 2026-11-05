namespace ec.edu.monster.pruebas.fakes;

/// <summary>
/// Response envelopes shaped like the ones SoapCore (XmlSerializer) returns for
/// WS_EUREKABANK_DOTNET_SOAP: namespace http://tempuri.org/, PascalCase fields, &lt;op&gt;Result wrapper.
/// </summary>
public static class SoapRespuestas
{
    public static string Sobre(string operacion, string campos)
        => "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\"><s:Body>"
         + $"<{operacion}Response xmlns=\"http://tempuri.org/\"><{operacion}Result>{campos}</{operacion}Result></{operacion}Response>"
         + "</s:Body></s:Envelope>";

    public static string LoginOk(string operacion, string nombre, string tipoUsuario, string token = "token-123")
        => Sobre(operacion,
            "<Estado>OK</Estado><Mensaje>Ingreso de sesión exitoso</Mensaje>"
            + $"<Token>{token}</Token><Nombre>{nombre}</Nombre><EstadoUsuario>ACTIVO</EstadoUsuario>"
            + $"<TipoUsuario>{tipoUsuario}</TipoUsuario><Perfil>{tipoUsuario.ToUpperInvariant()}</Perfil>");

    public static string Error(string operacion, string mensaje)
        => Sobre(operacion, $"<Estado>ERROR</Estado><Mensaje>{mensaje}</Mensaje>");

    public static string MovimientoOk(string operacion, string mensaje, int numero, string nuevoSaldo)
        => Sobre(operacion,
            $"<Estado>OK</Estado><Mensaje>{mensaje}</Mensaje><IdMovimiento>{numero}</IdMovimiento>"
            + $"<NumeroMovimiento>{numero}</NumeroMovimiento><NuevoSaldo>{nuevoSaldo}</NuevoSaldo>");

    public static string ConsultaOk(params (int Numero, string Fecha, string Tipo, string Accion, string Importe, string Referencia)[] movimientos)
        => Sobre("consultarMovimientos",
            "<Estado>OK</Estado><Mensaje>Consulta de movimientos exitosa</Mensaje>"
            + "<Cuenta><NumeroCuenta>00100001</NumeroCuenta><Titular>Ana Pérez</Titular><Moneda>USD</Moneda>"
            + "<Saldo>379.50</Saldo><EstadoCuenta>ACTIVO</EstadoCuenta></Cuenta>"
            + "<Movimientos>"
            + string.Concat(movimientos.Select(m =>
                $"<MovimientoDTO><NumeroMovimiento>{m.Numero}</NumeroMovimiento><Fecha>{m.Fecha}</Fecha>"
                + $"<TipoMovimiento>{m.Tipo}</TipoMovimiento><Accion>{m.Accion}</Accion><Importe>{m.Importe}</Importe>"
                + $"<CuentaReferencia>{m.Referencia}</CuentaReferencia></MovimientoDTO>"))
            + "</Movimientos>");

    public static string Falla(string mensaje)
        => "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\"><s:Body><s:Fault>"
         + $"<faultcode>s:Client</faultcode><faultstring>{mensaje}</faultstring></s:Fault></s:Body></s:Envelope>";
}
