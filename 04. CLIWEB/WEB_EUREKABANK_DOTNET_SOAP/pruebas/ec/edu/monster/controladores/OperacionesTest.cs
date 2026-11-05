using System.Net;
using ec.edu.monster.fakes;

namespace ec.edu.monster.controladores;

/// <summary>Depósito, Retiro and Transferencia: review, confirm, receipt (PRG) and role rules.</summary>
public class OperacionesTest : IDisposable
{
    private readonly AppPrueba _app = new();

    public void Dispose() => _app.Dispose();

    // ---------- Depósito ----------

    [Fact]
    public async Task AClienteCannotOpenDeposito()
    {
        Navegador navegador = _app.Navegador();
        await navegador.LoginClienteAsync();

        HttpResponseMessage respuesta = await navegador.GetAsync("/deposito");

        Assert.Equal("/menu", respuesta.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task DepositoHasNoEmployeeCodeField()
    {
        Navegador navegador = _app.Navegador();
        await navegador.LoginEmpleadoAsync();

        string html = await navegador.HtmlAsync("/deposito");

        Assert.Contains("Cuenta destino", html);
        Assert.DoesNotContain("empleado", html.Replace("Empleado", ""), StringComparison.Ordinal);
    }

    [Fact]
    public async Task DepositoReviewShowsTheConfirmationWithoutCallingTheServer()
    {
        Navegador navegador = _app.Navegador();
        await navegador.LoginEmpleadoAsync();

        string html = await navegador.PostHtmlAsync("/deposito", ("paso", "revisar"), ("cuenta", " 00100001 "), ("importe", "1250,5"));

        Assert.Contains("Confirmar depósito", html);
        Assert.Contains("¿Depositar <strong>$ 1,250.50</strong>", html);
        Assert.Contains("name=\"cuenta\" value=\"00100001\"", html);
        Assert.Contains("name=\"importe\" value=\"1250.5\"", html);
        Assert.Equal(0, _app.Transporte.Llamadas("registrarDeposito"));
    }

    [Fact]
    public async Task DepositoShowsTheFieldErrors()
    {
        Navegador navegador = _app.Navegador();
        await navegador.LoginEmpleadoAsync();

        string html = await navegador.PostHtmlAsync("/deposito", ("paso", "revisar"), ("cuenta", ""), ("importe", "0"));

        Assert.Contains("Ingrese la cuenta de destino", html);
        Assert.Contains(MovimientoForm.ImporteInvalido, html);
        Assert.Contains("field__input--error", html);
    }

    [Fact]
    public async Task DepositoConfirmRegistersRedirectsAndShowsTheReceiptOnce()
    {
        Navegador navegador = _app.Navegador();
        await navegador.LoginEmpleadoAsync();
        _app.Transporte.Responder("registrarDeposito",
            SoapRespuestas.MovimientoOk("registrarDeposito", "Depósito registrado exitosamente", 12, "1629.50"));

        HttpResponseMessage confirmacion = await navegador.PostAsync("/deposito",
            ("paso", "confirmar"), ("cuenta", "00100001"), ("importe", "1250.5"));
        string recibo = await navegador.HtmlAsync("/deposito");
        string despues = await navegador.HtmlAsync("/deposito");

        Assert.Equal("/deposito", confirmacion.Headers.Location?.OriginalString);
        PeticionSoap peticion = _app.Transporte.Ultima("registrarDeposito");
        Assert.Equal(("token-123", "00100001", "1250.50"), (peticion.Parametro("token"), peticion.Parametro("cuenta"), peticion.Parametro("importe")));
        Assert.Contains("Depósito registrado exitosamente", recibo);
        Assert.Contains("<dt>N.º de movimiento</dt><dd>12</dd>", recibo);
        Assert.Contains("<dt>Nuevo saldo</dt><dd>$ 1,629.50</dd>", recibo);
        Assert.Contains("<dt>Importe</dt><dd>$ 1,250.50</dd>", recibo);
        Assert.Contains("badge__platform--dotnet", recibo);
        Assert.DoesNotContain("Comprobante", despues);
    }

    [Fact]
    public async Task DepositoShowsTheServerErrorOnTheForm()
    {
        Navegador navegador = _app.Navegador();
        await navegador.LoginEmpleadoAsync();
        _app.Transporte.Responder("registrarDeposito", SoapRespuestas.Error("registrarDeposito", "La cuenta no existe"));

        string html = await navegador.PostHtmlAsync("/deposito", ("paso", "confirmar"), ("cuenta", "999"), ("importe", "10"));

        Assert.Contains("<p class=\"alert alert--error\" role=\"alert\">La cuenta no existe</p>", html);
        Assert.Contains("value=\"999\"", html);
    }

    [Fact]
    public async Task AnExpiredTokenEndsTheSessionAndGoesBackToTheLogin()
    {
        Navegador navegador = _app.Navegador();
        await navegador.LoginEmpleadoAsync();
        _app.Transporte.Responder("registrarDeposito", SoapRespuestas.Error("registrarDeposito", "Token inválido o sesión expirada"));

        HttpResponseMessage respuesta = await navegador.PostAsync("/deposito", ("paso", "confirmar"), ("cuenta", "1"), ("importe", "10"));
        HttpResponseMessage menu = await navegador.GetAsync("/menu");

        Assert.Equal("/login?expirada=1", respuesta.Headers.Location?.OriginalString);
        Assert.Equal("/login", menu.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task EditarGoesBackToTheFormKeepingTheValues()
    {
        Navegador navegador = _app.Navegador();
        await navegador.LoginEmpleadoAsync();

        string html = await navegador.PostHtmlAsync("/deposito", ("paso", "editar"), ("cuenta", "00100001"), ("importe", "10"));

        Assert.Contains("value=\"00100001\"", html);
        Assert.Contains("value=\"10\"", html);
        Assert.Contains("Revisar", html);
    }

    // ---------- Retiro ----------

    [Fact]
    public async Task AnEmpleadoMustTypeTheAccountKeyToConfirmARetiro()
    {
        Navegador navegador = _app.Navegador();
        await navegador.LoginEmpleadoAsync();

        string confirmacion = await navegador.PostHtmlAsync("/retiro", ("paso", "revisar"), ("cuenta", "00100001"), ("importe", "50"));
        string sinClave = await navegador.PostHtmlAsync("/retiro", ("paso", "confirmar"), ("cuenta", "00100001"), ("importe", "50"), ("clave", " "));

        Assert.Contains("Clave de la cuenta", confirmacion);
        Assert.Contains("type=\"password\" id=\"clave\" name=\"clave\"", confirmacion);
        Assert.Contains(RetiroController.ClaveVacia, sinClave);
        Assert.Equal(0, _app.Transporte.Llamadas("registrarRetiro"));
    }

    [Fact]
    public async Task AnEmpleadoRetiroSendsTheKeyAsTypedAndShowsTheReceipt()
    {
        Navegador navegador = _app.Navegador();
        await navegador.LoginEmpleadoAsync();
        _app.Transporte.Responder("registrarRetiro", SoapRespuestas.MovimientoOk("registrarRetiro", "Retiro registrado exitosamente", 13, "329.50"));

        await navegador.PostAsync("/retiro", ("paso", "confirmar"), ("cuenta", "00100001"), ("importe", "50"), ("clave", " 123456 "));
        string recibo = await navegador.HtmlAsync("/retiro");

        Assert.Equal(" 123456 ", _app.Transporte.Ultima("registrarRetiro").Parametro("clave"));
        Assert.Contains("Retiro registrado exitosamente", recibo);
        Assert.Contains("<dt>Cuenta</dt><dd>00100001</dd>", recibo);
        Assert.DoesNotContain("123456", recibo);
    }

    [Fact]
    public async Task AClienteRetiroUsesTheSessionAccountWithReadOnlyFieldAndEmptyKey()
    {
        Navegador navegador = _app.Navegador();
        await navegador.LoginClienteAsync("00100001");
        _app.Transporte.Responder("registrarRetiro", SoapRespuestas.MovimientoOk("registrarRetiro", "Retiro registrado exitosamente", 14, "279.50"));

        string formulario = await navegador.HtmlAsync("/retiro");
        string confirmacion = await navegador.PostHtmlAsync("/retiro", ("paso", "revisar"), ("cuenta", "99999999"), ("importe", "50"));
        await navegador.PostAsync("/retiro", ("paso", "confirmar"), ("cuenta", "99999999"), ("importe", "50"), ("clave", "x"));

        Assert.Contains("value=\"00100001\"", formulario);
        Assert.Contains("readonly=\"readonly\"", formulario);
        Assert.Contains("de la cuenta <strong>00100001</strong>", confirmacion);
        Assert.DoesNotContain("id=\"clave\"", confirmacion);
        PeticionSoap peticion = _app.Transporte.Ultima("registrarRetiro");
        Assert.Equal(("00100001", ""), (peticion.Parametro("cuenta"), peticion.Parametro("clave")));
    }

    // ---------- Transferencia ----------

    [Fact]
    public async Task ATransferNeedsDifferentAccounts()
    {
        Navegador navegador = _app.Navegador();
        await navegador.LoginEmpleadoAsync();

        string html = await navegador.PostHtmlAsync("/transferencia", ("paso", "revisar"), ("origen", "001"), ("destino", "001"), ("importe", "5"));

        Assert.Contains(MovimientoForm.CuentasIguales, html);
    }

    [Fact]
    public async Task AnEmpleadoTransferSendsBothAccountsAndTheSourceKey()
    {
        Navegador navegador = _app.Navegador();
        await navegador.LoginEmpleadoAsync();
        _app.Transporte.Responder("registrarTransferencia",
            SoapRespuestas.MovimientoOk("registrarTransferencia", "Transferencia registrada exitosamente", 15, "200.00"));

        string confirmacion = await navegador.PostHtmlAsync("/transferencia", ("paso", "revisar"), ("origen", "001"), ("destino", "002"), ("importe", "5"));
        await navegador.PostAsync("/transferencia", ("paso", "confirmar"), ("origen", "001"), ("destino", "002"), ("importe", "5"), ("clave", "k1"));
        string recibo = await navegador.HtmlAsync("/transferencia");

        Assert.Contains("Clave de la cuenta de origen", confirmacion);
        Assert.Contains("a la cuenta <strong>002</strong>", confirmacion);
        PeticionSoap peticion = _app.Transporte.Ultima("registrarTransferencia");
        Assert.Equal(("001", "002", "5.00", "k1"),
            (peticion.Parametro("cuentaOrigen"), peticion.Parametro("cuentaDestino"), peticion.Parametro("importe"), peticion.Parametro("clave")));
        Assert.Contains("<dt>Cuenta de origen</dt><dd>001</dd>", recibo);
        Assert.Contains("<dt>Cuenta de destino</dt><dd>002</dd>", recibo);
    }

    [Fact]
    public async Task AClienteTransferAlwaysLeavesFromTheSessionAccountWithoutKey()
    {
        Navegador navegador = _app.Navegador();
        await navegador.LoginClienteAsync("00100001");
        _app.Transporte.Responder("registrarTransferencia",
            SoapRespuestas.MovimientoOk("registrarTransferencia", "Transferencia registrada exitosamente", 16, "100.00"));

        await navegador.PostAsync("/transferencia", ("paso", "confirmar"), ("origen", "77777777"), ("destino", "002"), ("importe", "5"), ("clave", "x"));

        PeticionSoap peticion = _app.Transporte.Ultima("registrarTransferencia");
        Assert.Equal(("00100001", ""), (peticion.Parametro("cuentaOrigen"), peticion.Parametro("clave")));
    }

    [Fact]
    public async Task TheConnectionErrorNamesTheServerAddress()
    {
        Navegador navegador = _app.Navegador();
        await navegador.LoginEmpleadoAsync();

        string html = await navegador.PostHtmlAsync("/transferencia",
            ("paso", "confirmar"), ("origen", "001"), ("destino", "002"), ("importe", "5"), ("clave", "k"));

        Assert.Contains($"No se pudo conectar con el servidor: http://{AppPrueba.Host}:{AppPrueba.Puerto}/MovimientoWebService.svc", html);
    }
}
