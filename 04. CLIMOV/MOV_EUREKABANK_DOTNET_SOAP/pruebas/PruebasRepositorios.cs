using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Data;
using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Enums;
using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Repositorios;

namespace MOV_EUREKABANK_DOTNET_SOAP.Pruebas;

public static class PruebasRepositorios
{
    private const string UrlBase = "http://servidor:5171/";

    private const string Movimiento1 = "<NumeroMovimiento>1</NumeroMovimiento><Fecha>2026-10-30T09:15:00</Fecha>"
        + "<TipoMovimiento>Depósito</TipoMovimiento><Accion>INGRESO</Accion><Importe>500.00</Importe>"
        + "<CuentaReferencia></CuentaReferencia>";
    private const string Movimiento2 = "<NumeroMovimiento>2</NumeroMovimiento><Fecha>2026-10-31T16:40:00</Fecha>"
        + "<TipoMovimiento>Transferencia</TipoMovimiento><Accion>SALIDA</Accion><Importe>120.50</Importe>"
        + "<CuentaReferencia>00100002</CuentaReferencia>";

    private static LoginSoapRepositorio Login(TransporteFalso transporte, bool sinRed = false)
        => new(UrlBase, transporte, new RedFalsa(sinRed));

    private static MovimientoSoapRepositorio Movimientos(TransporteFalso transporte)
        => new(UrlBase, transporte, new RedFalsa(false));

    public static Ejecutor Crear()
    {
        var e = new Ejecutor();

        e.Caso("loginEmpleado OK: token, perfil y nombre; sobre, URL y SOAPAction", async () =>
        {
            var transporte = TransporteFalso.Responde(TransporteFalso.Respuesta("loginEmpleado",
                "<Estado>OK</Estado><Mensaje>Ingreso de sesión exitoso</Mensaje><Token>abc-123</Token>"
                + "<Perfil>EMPLEADO</Perfil><Nombre>Marcos Escobar</Nombre>"));

            Resultado<Sesion> r = await Login(transporte).LoginEmpleadoAsync("mescobar", "secreta");

            Afirmar.Verdadero(r.Exito, r.Mensaje);
            Afirmar.Igual("abc-123", r.Datos!.Token);
            Afirmar.Igual(Perfil.Empleado, r.Datos.Perfil);
            Afirmar.Igual("Marcos Escobar", r.Datos.Nombre);
            Afirmar.Igual(UrlBase + "LoginWebService.svc", transporte.Ultima.Url);
            Afirmar.Igual("http://tempuri.org/ILoginWebService/loginEmpleado", transporte.Ultima.Accion);
            Afirmar.Contiene("<loginEmpleado xmlns=\"http://tempuri.org/\"><usuario>mescobar</usuario><clave>secreta</clave></loginEmpleado>",
                transporte.Ultima.Sobre);
        });
        e.Caso("loginCliente guarda la cuenta de la sesión", async () =>
        {
            var transporte = TransporteFalso.Responde(TransporteFalso.Respuesta("loginCliente",
                "<Estado>OK</Estado><Mensaje>ok</Mensaje><Token>t</Token><Perfil>CLIENTE</Perfil><Nombre>Ana</Nombre>"));

            Resultado<Sesion> r = await Login(transporte).LoginClienteAsync("00100001", "123456");

            Afirmar.Igual(Perfil.Cliente, r.Datos!.Perfil);
            Afirmar.Igual("00100001", r.Datos.Cuenta);
            Afirmar.Contiene("<cuenta>00100001</cuenta><clave>123456</clave>", transporte.Ultima.Sobre);
        });
        e.Caso("Login con respuesta estilo JAX-WS (minúsculas, otro namespace) también se lee", async () =>
        {
            var transporte = TransporteFalso.Responde("<S:Envelope xmlns:S=\"http://schemas.xmlsoap.org/soap/envelope/\"><S:Body>"
                + "<ns2:loginEmpleadoResponse xmlns:ns2=\"http://ws.monster.edu.ec/\"><return><estado>OK</estado>"
                + "<mensaje>ok</mensaje><token>t</token><perfil>EMPLEADO</perfil><nombre>N</nombre></return>"
                + "</ns2:loginEmpleadoResponse></S:Body></S:Envelope>");

            Afirmar.Verdadero((await Login(transporte).LoginEmpleadoAsync("u", "c")).Exito, "debía iniciar sesión");
        });
        e.Caso("Login con credenciales incorrectas muestra el mensaje del servidor", async () =>
        {
            var transporte = TransporteFalso.Responde(TransporteFalso.Respuesta("loginCliente",
                "<Estado>ERROR</Estado><Mensaje>La cuenta no se encuentra activa</Mensaje>"));

            Resultado<Sesion> r = await Login(transporte).LoginClienteAsync("00100001", "123456");

            Afirmar.Falso(r.Exito, "debía fallar");
            Afirmar.Igual("La cuenta no se encuentra activa", r.Mensaje);
        });
        e.Caso("Login OK sin token es respuesta incompleta", async () =>
        {
            var transporte = TransporteFalso.Responde(TransporteFalso.Respuesta("loginEmpleado",
                "<Estado>OK</Estado><Mensaje>ok</Mensaje><Token></Token>"));

            Afirmar.Igual(SoapErrores.RespuestaIncompleta, (await Login(transporte).LoginEmpleadoAsync("u", "c")).Mensaje);
        });
        e.Caso("Login sin respuesta en 15 s informa que no hay servidor", async () =>
        {
            var transporte = TransporteFalso.Lanza(new TaskCanceledException("timeout"));

            Afirmar.EmpiezaCon(SoapErrores.SinServidor, (await Login(transporte).LoginEmpleadoAsync("u", "c")).Mensaje);
        });
        e.Caso("Login sin red no llama al servidor", async () =>
        {
            var transporte = TransporteFalso.Responde("");

            Afirmar.Igual(SoapErrores.SinConexion, (await Login(transporte, sinRed: true).LoginEmpleadoAsync("u", "c")).Mensaje);
            Afirmar.Igual(0, transporte.Peticiones.Count, "peticiones");
        });
        e.Caso("SOAP Fault: mensaje genérico, no se toma como éxito", async () =>
        {
            var transporte = TransporteFalso.Responde(TransporteFalso.Falla("Object reference not set"));

            Afirmar.EmpiezaCon("El servidor no pudo procesar la solicitud", (await Login(transporte).LoginEmpleadoAsync("u", "c")).Mensaje);
        });
        e.Caso("consultarMovimientos lee la cuenta y sus movimientos", async () =>
        {
            var transporte = TransporteFalso.Responde(TransporteFalso.Respuesta("consultarMovimientos",
                "<Estado>OK</Estado><Mensaje>Consulta de movimientos exitosa</Mensaje>"
                + "<Cuenta><NumeroCuenta>00100001</NumeroCuenta><Titular>Ana Pérez</Titular><Moneda>USD</Moneda>"
                + "<Saldo>379.50</Saldo><EstadoCuenta>ACTIVO</EstadoCuenta></Cuenta>"
                + $"<Movimientos><MovimientoDTO>{Movimiento1}</MovimientoDTO><MovimientoDTO>{Movimiento2}</MovimientoDTO></Movimientos>"));

            Resultado<ConsultaMovimientos> r = await Movimientos(transporte).ConsultarMovimientosAsync("tok", "00100001");

            Afirmar.Verdadero(r.Exito, r.Mensaje);
            Afirmar.Igual("Ana Pérez", r.Datos!.Cuenta.Titular);
            Afirmar.Igual(379.50m, r.Datos.Cuenta.Saldo);
            Afirmar.Igual(2, r.Datos.Movimientos.Count);
            Movimiento transferencia = r.Datos.Movimientos[1];
            Afirmar.Falso(transferencia.EsIngreso, "es salida");
            Afirmar.Igual("00100002", transferencia.CuentaReferencia);
            Afirmar.Igual(UrlBase + "MovimientoWebService.svc", transporte.Ultima.Url);
            Afirmar.Igual("http://tempuri.org/IMovimientoWebService/consultarMovimientos", transporte.Ultima.Accion);
            Afirmar.Contiene("<token>tok</token><cuenta>00100001</cuenta>", transporte.Ultima.Sobre);
        });
        e.Caso("consultarMovimientos de una cuenta sin movimientos devuelve lista vacía", async () =>
        {
            var transporte = TransporteFalso.Responde(TransporteFalso.Respuesta("consultarMovimientos",
                "<Estado>OK</Estado><Mensaje>ok</Mensaje><Cuenta><NumeroCuenta>00100001</NumeroCuenta></Cuenta>"));

            Afirmar.Igual(0, (await Movimientos(transporte).ConsultarMovimientosAsync("tok", "00100001")).Datos!.Movimientos.Count);
        });
        e.Caso("Token inválido marca la sesión como expirada", async () =>
        {
            var transporte = TransporteFalso.Responde(TransporteFalso.Respuesta("consultarMovimientos",
                "<Estado>ERROR</Estado><Mensaje>Token inválido o sesión expirada</Mensaje>"));

            Afirmar.Verdadero((await Movimientos(transporte).ConsultarMovimientosAsync("tok", "00100001")).SesionExpirada, "expirada");
        });
        e.Caso("registrarTransferencia envía las dos cuentas, el importe y la clave", async () =>
        {
            var transporte = TransporteFalso.Responde(TransporteFalso.Respuesta("registrarTransferencia",
                "<Estado>OK</Estado><Mensaje>Transferencia registrada correctamente</Mensaje>"
                + "<NumeroMovimiento>15</NumeroMovimiento><NuevoSaldo>879.50</NuevoSaldo>"));
            var solicitud = new SolicitudOperacion(TipoOperacion.Transferencia, "00100001", "00100002", 120.50m, "123456");

            Resultado<Comprobante> r = await Movimientos(transporte).RegistrarAsync("tok", solicitud);

            Afirmar.Verdadero(r.Exito, r.Mensaje);
            Afirmar.Igual("15", r.Datos!.NumeroMovimiento);
            Afirmar.Igual(879.50m, r.Datos.NuevoSaldo);
            Afirmar.Igual("http://tempuri.org/IMovimientoWebService/registrarTransferencia", transporte.Ultima.Accion);
            Afirmar.Contiene("<registrarTransferencia xmlns=\"http://tempuri.org/\"><token>tok</token>"
                + "<cuentaOrigen>00100001</cuentaOrigen><cuentaDestino>00100002</cuentaDestino>"
                + "<importe>120.50</importe><clave>123456</clave></registrarTransferencia>", transporte.Ultima.Sobre);
        });
        e.Caso("registrarDeposito no envía clave e importe con dos decimales", async () =>
        {
            var transporte = TransporteFalso.Responde(TransporteFalso.Respuesta("registrarDeposito",
                "<Estado>OK</Estado><Mensaje>ok</Mensaje><NumeroMovimiento>3</NumeroMovimiento><NuevoSaldo>10</NuevoSaldo>"));
            var solicitud = new SolicitudOperacion(TipoOperacion.Deposito, "00100001", null, 10m, "");

            await Movimientos(transporte).RegistrarAsync("tok", solicitud);

            Afirmar.Contiene("<registrarDeposito xmlns=\"http://tempuri.org/\"><token>tok</token>"
                + "<cuenta>00100001</cuenta><importe>10.00</importe></registrarDeposito>", transporte.Ultima.Sobre);
        });
        e.Caso("Error de negocio: se muestra el mensaje del servidor", async () =>
        {
            var transporte = TransporteFalso.Responde(TransporteFalso.Respuesta("registrarRetiro",
                "<Estado>ERROR</Estado><Mensaje>Saldo insuficiente</Mensaje>"));
            var solicitud = new SolicitudOperacion(TipoOperacion.Retiro, "00100001", null, 9999.00m, "123456");

            Resultado<Comprobante> r = await Movimientos(transporte).RegistrarAsync("tok", solicitud);

            Afirmar.Falso(r.Exito, "debía fallar");
            Afirmar.Falso(r.SesionExpirada, "no es sesión expirada");
            Afirmar.Igual("Saldo insuficiente", r.Mensaje);
        });

        return e;
    }
}
