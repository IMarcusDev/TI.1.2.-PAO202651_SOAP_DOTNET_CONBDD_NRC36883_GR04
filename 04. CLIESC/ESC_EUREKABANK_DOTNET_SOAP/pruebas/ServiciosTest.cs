using ec.edu.monster.config;
using ec.edu.monster.modelo;
using ec.edu.monster.modelo.dto;
using ec.edu.monster.modelo.servicios;
using ec.edu.monster.pruebas.fakes;

namespace ec.edu.monster.pruebas;

public class LoginServicioTest
{
    private readonly FakeTransporte _transporte = new();
    private readonly LoginServicio _servicio;

    public LoginServicioTest()
        => _servicio = new LoginServicio(_transporte, new Configuracion("10.0.0.5", 5171));

    [Fact]
    public async Task LoginEmpleadoSendsUsuarioAndClaveToTheLoginServiceAndReadsTheUser()
    {
        _transporte.Responder("loginEmpleado", SoapRespuestas.LoginOk("loginEmpleado", "Monster Bank", "Empleado"));

        LoginResultado resultado = await _servicio.LoginEmpleadoAsync("monster", "monster9");

        PeticionSoap peticion = _transporte.Ultima("loginEmpleado");
        Assert.Equal("http://10.0.0.5:5171/LoginWebService.svc", peticion.Url);
        Assert.Equal("http://tempuri.org/ILoginWebService/loginEmpleado", peticion.Accion);
        Assert.Equal("monster", peticion.Parametro("usuario"));
        Assert.Equal("monster9", peticion.Parametro("clave"));
        Assert.Equal(new LoginResultado(true, "Ingreso de sesión exitoso", "token-123", "Monster Bank", "Empleado"), resultado);
    }

    [Fact]
    public async Task LoginClienteSendsTheAccountNumber()
    {
        _transporte.Responder("loginCliente", SoapRespuestas.LoginOk("loginCliente", "Ana Pérez", "Cliente"));

        LoginResultado resultado = await _servicio.LoginClienteAsync("00100001", "123456");

        Assert.Equal("00100001", _transporte.Ultima("loginCliente").Parametro("cuenta"));
        Assert.Equal("Cliente", resultado.TipoUsuario);
    }

    [Fact]
    public async Task TheLegacyPerfilIsNormalizedWhenTipoUsuarioIsMissing()
    {
        _transporte.Responder("loginEmpleado", SoapRespuestas.Sobre("loginEmpleado",
            "<Estado>OK</Estado><Token>t</Token><Nombre>Ana</Nombre><Perfil>CLIENTE</Perfil>"));

        Assert.Equal(Sesion.RolCliente, (await _servicio.LoginEmpleadoAsync("00100001", "k")).TipoUsuario);
    }

    [Fact]
    public async Task ReturnsTheServerMessageOnError()
    {
        _transporte.Responder("loginEmpleado", SoapRespuestas.Error("loginEmpleado", "Usuario o clave incorrectos"));

        Assert.Equal(LoginResultado.Error("Usuario o clave incorrectos"), await _servicio.LoginEmpleadoAsync("x", "y"));
    }

    [Fact]
    public async Task ReportsTheServerAddressWhenItCannotConnect()
    {
        LoginResultado resultado = await _servicio.LoginEmpleadoAsync("x", "y");

        Assert.Equal(LoginResultado.Error("No se pudo conectar con el servidor: http://10.0.0.5:5171/LoginWebService.svc"), resultado);
    }

    [Fact]
    public async Task ShowsAGenericMessageForASoapFault()
    {
        _transporte.Responder("loginEmpleado", SoapRespuestas.Falla("NullReferenceException"));

        Assert.Equal(LoginResultado.Error(ServicioSoap.FallaServidor), await _servicio.LoginEmpleadoAsync("x", "y"));
    }

    [Fact]
    public async Task TreatsAnOkWithoutTokenAsAnIncompleteResponse()
    {
        _transporte.Responder("loginEmpleado", SoapRespuestas.Sobre("loginEmpleado", "<Estado>OK</Estado>"));

        Assert.Equal(LoginResultado.Error(ServicioSoap.RespuestaIncompleta), await _servicio.LoginEmpleadoAsync("x", "y"));
    }

    [Fact]
    public async Task LogoutSendsTheToken()
    {
        _transporte.Responder("logout", SoapRespuestas.Sobre("logout", "<Estado>OK</Estado><Mensaje>Sesión cerrada</Mensaje>"));

        Assert.True(await _servicio.LogoutAsync("token-123"));
        Assert.Equal("token-123", _transporte.Ultima("logout").Parametro("token"));
    }
}

public class MovimientoServicioTest
{
    private readonly FakeTransporte _transporte = new();
    private readonly MovimientoServicio _servicio;

    public MovimientoServicioTest()
        => _servicio = new MovimientoServicio(_transporte, new Configuracion());

    [Fact]
    public async Task RegistrarDepositoSendsTokenAccountAndAmountWithoutKey()
    {
        _transporte.Responder("registrarDeposito", SoapRespuestas.MovimientoOk("registrarDeposito", "Depósito registrado exitosamente", 7, "620.00"));

        MovimientoResultado resultado = await _servicio.RegistrarDepositoAsync("t", "00100001", 120.5m);

        PeticionSoap peticion = _transporte.Ultima("registrarDeposito");
        Assert.Equal("http://localhost:5171/MovimientoWebService.svc", peticion.Url);
        Assert.Equal("http://tempuri.org/IMovimientoWebService/registrarDeposito", peticion.Accion);
        Assert.Equal(("t", "00100001", "120.50", (string?)null),
            (peticion.Parametro("token"), peticion.Parametro("cuenta"), peticion.Parametro("importe"), peticion.Parametro("clave")));
        Assert.Equal(new MovimientoResultado(true, "Depósito registrado exitosamente", "7", 620.00m), resultado);
    }

    [Fact]
    public async Task RegistrarRetiroSendsTheKeyAsTyped()
    {
        _transporte.Responder("registrarRetiro", SoapRespuestas.MovimientoOk("registrarRetiro", "ok", 8, "500.00"));

        await _servicio.RegistrarRetiroAsync("t", "00100001", 10m, " clave ");

        Assert.Equal(" clave ", _transporte.Ultima("registrarRetiro").Parametro("clave"));
    }

    [Fact]
    public async Task RegistrarTransferenciaSendsBothAccounts()
    {
        _transporte.Responder("registrarTransferencia", SoapRespuestas.MovimientoOk("registrarTransferencia", "ok", 9, "400.00"));

        await _servicio.RegistrarTransferenciaAsync("t", "00100001", "00200002", 99.99m, "");

        PeticionSoap peticion = _transporte.Ultima("registrarTransferencia");
        Assert.Equal(("00100001", "00200002", "99.99", ""),
            (peticion.Parametro("cuentaOrigen"), peticion.Parametro("cuentaDestino"), peticion.Parametro("importe"), peticion.Parametro("clave")));
    }

    [Fact]
    public async Task AnInvalidTokenMarksTheSessionAsExpired()
    {
        _transporte.Responder("registrarRetiro", SoapRespuestas.Error("registrarRetiro", "Token inválido o sesión expirada"));

        MovimientoResultado resultado = await _servicio.RegistrarRetiroAsync("t", "1", 1m, "k");

        Assert.Equal(MovimientoResultado.Error("Token inválido o sesión expirada", sesionExpirada: true), resultado);
    }

    [Fact]
    public async Task ABusinessErrorKeepsTheServerMessage()
    {
        _transporte.Responder("registrarRetiro", SoapRespuestas.Error("registrarRetiro", "Saldo insuficiente"));

        Assert.Equal(MovimientoResultado.Error("Saldo insuficiente"), await _servicio.RegistrarRetiroAsync("t", "1", 1m, "k"));
    }

    [Fact]
    public async Task ConsultarMovimientosReadsTheAccountAndFormatsTheRowsNewestFirst()
    {
        _transporte.Responder("consultarMovimientos", SoapRespuestas.ConsultaOk(
            (1, "2026-10-30T09:15:00", "Depósito", "INGRESO", "500.00", ""),
            (6, "2026-10-31T16:40:00", "Transferencia", "SALIDA", "120.50", "00200002")));

        HistorialCuenta historial = await _servicio.ConsultarMovimientosAsync("t", "00100001");

        Assert.Equal("00100001", _transporte.Ultima("consultarMovimientos").Parametro("cuenta"));
        Assert.True(historial.Ok);
        Assert.Equal(new CuentaConsultada("00100001", "Ana Pérez", "USD", 379.50m, "ACTIVO"), historial.Cuenta);
        Assert.Equal(
            [
                new MovimientoConsultado("000006", "31/10/2026 16:40", "Transferencia", "SALIDA", 120.50m, "00200002"),
                new MovimientoConsultado("000001", "30/10/2026 09:15", "Depósito", "INGRESO", 500.00m, "-"),
            ],
            historial.Movimientos);
    }

    [Fact]
    public async Task ConsultarMovimientosWithAnInvalidTokenIsAnExpiredSession()
    {
        _transporte.Responder("consultarMovimientos", SoapRespuestas.Error("consultarMovimientos", "Token inválido o sesión expirada"));

        HistorialCuenta historial = await _servicio.ConsultarMovimientosAsync("t", "1");

        Assert.False(historial.Ok);
        Assert.True(historial.SesionExpirada);
    }
}
