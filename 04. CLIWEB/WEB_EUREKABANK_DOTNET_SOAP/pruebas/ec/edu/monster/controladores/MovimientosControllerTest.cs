using ec.edu.monster.fakes;

namespace ec.edu.monster.controladores;

/// <summary>"Consultar movimientos" for both roles.</summary>
public class MovimientosControllerTest : IDisposable
{
    private readonly AppPrueba _app = new();

    public void Dispose() => _app.Dispose();

    [Fact]
    public async Task AnEmpleadoFirstSeesOnlyTheSearchForm()
    {
        Navegador navegador = _app.Navegador();
        await navegador.LoginEmpleadoAsync();

        string html = await navegador.HtmlAsync("/movimientos");

        Assert.Contains("id=\"cuenta\" name=\"cuenta\"", html);
        Assert.DoesNotContain("Datos de la cuenta", html);
        Assert.Equal(0, _app.Transporte.Llamadas("consultarMovimientos"));
    }

    [Fact]
    public async Task AnEmptyAccountIsRejected()
    {
        Navegador navegador = _app.Navegador();
        await navegador.LoginEmpleadoAsync();

        string html = await navegador.HtmlAsync("/movimientos?cuenta=%20");

        Assert.Contains(MovimientosController.CuentaVacia, html);
        Assert.Equal(0, _app.Transporte.Llamadas("consultarMovimientos"));
    }

    [Fact]
    public async Task ShowsTheAccountAndTheMovementsNewestFirst()
    {
        Navegador navegador = _app.Navegador();
        await navegador.LoginEmpleadoAsync();
        _app.Transporte.Responder("consultarMovimientos", SoapRespuestas.ConsultaOk(
            (1, "2026-10-30T09:15:00", "Depósito", "INGRESO", "500.00", ""),
            (6, "2026-10-31T16:40:00", "Transferencia", "SALIDA", "120.50", "00200002")));

        string html = await navegador.HtmlAsync("/movimientos?cuenta=00100001");

        Assert.Equal("00100001", _app.Transporte.Ultima("consultarMovimientos").Parametro("cuenta"));
        Assert.Contains("<dt>Titular</dt><dd>Ana Pérez</dd>", html);
        Assert.Contains("<dt>Saldo</dt><dd>$ 379.50</dd>", html);
        Assert.True(html.IndexOf("000006", StringComparison.Ordinal) < html.IndexOf("000001", StringComparison.Ordinal));
        Assert.Contains("<span class=\"movimiento--egreso\">SALIDA</span>", html);
        Assert.Contains("<span class=\"movimiento--ingreso\">INGRESO</span>", html);
        Assert.Contains("31/10/2026 16:40", html);
        Assert.Contains("/css/movimientos.css", html);
    }

    [Fact]
    public async Task AnAccountWithoutMovementsSaysSo()
    {
        Navegador navegador = _app.Navegador();
        await navegador.LoginEmpleadoAsync();
        _app.Transporte.Responder("consultarMovimientos", SoapRespuestas.ConsultaOk());

        string html = await navegador.HtmlAsync("/movimientos?cuenta=00100001");

        Assert.Contains("La cuenta no tiene movimientos", html);
    }

    [Fact]
    public async Task ShowsTheServerError()
    {
        Navegador navegador = _app.Navegador();
        await navegador.LoginEmpleadoAsync();
        _app.Transporte.Responder("consultarMovimientos", SoapRespuestas.Error("consultarMovimientos", "La cuenta no existe"));

        string html = await navegador.HtmlAsync("/movimientos?cuenta=999");

        Assert.Contains("La cuenta no existe", html);
        Assert.DoesNotContain("Datos de la cuenta", html);
    }

    [Fact]
    public async Task AClienteAlwaysSeesTheirOwnAccount()
    {
        Navegador navegador = _app.Navegador();
        await navegador.LoginClienteAsync("00100001");
        _app.Transporte.Responder("consultarMovimientos", SoapRespuestas.ConsultaOk());

        string html = await navegador.HtmlAsync("/movimientos?cuenta=99999999");

        Assert.Equal("00100001", _app.Transporte.Ultima("consultarMovimientos").Parametro("cuenta"));
        Assert.Contains("Su cuenta", html);
        Assert.DoesNotContain("id=\"cuenta\"", html);
    }

    [Fact]
    public async Task AnExpiredTokenGoesBackToTheLogin()
    {
        Navegador navegador = _app.Navegador();
        await navegador.LoginEmpleadoAsync();
        _app.Transporte.Responder("consultarMovimientos", SoapRespuestas.Error("consultarMovimientos", "Token inválido o sesión expirada"));

        HttpResponseMessage respuesta = await navegador.GetAsync("/movimientos?cuenta=1");

        Assert.Equal("/login?expirada=1", respuesta.Headers.Location?.OriginalString);
    }
}
