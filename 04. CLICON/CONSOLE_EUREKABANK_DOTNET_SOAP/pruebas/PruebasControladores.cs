//revisado y aprovado por Marcos Escobar
using System.Net;
using System.Text;
using System.Xml.Linq;
using CONSOLE_EUREKABANK_DOTNET_SOAP.Models.Enums;
using CONSOLE_EUREKABANK_DOTNET_SOAP.Controllers;
using CONSOLE_EUREKABANK_DOTNET_SOAP.Models.HTTP;

namespace CONSOLE_EUREKABANK_DOTNET_SOAP.Pruebas;

/// <summary>Reemplaza la red: guarda la última petición y devuelve la respuesta preparada.</summary>
internal sealed class ServidorFalso : HttpMessageHandler
{
    public string Url = "";
    public string Accion = "";
    public string Cuerpo = "";
    public string Respuesta = Sobre("<r><Estado>OK</Estado><Mensaje>ok</Mensaje></r>");
    public HttpStatusCode Codigo = HttpStatusCode.OK;
    public bool SinRed;

    public static string Sobre(string respuesta) =>
        "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\"><s:Body>" + respuesta + "</s:Body></s:Envelope>";

    /// <summary>El elemento de la operación dentro del soap:Body.</summary>
    public XElement Operacion => XDocument.Parse(Cuerpo).Root!.Elements().First().Elements().First();

    public string? Parametro(string nombre) => Operacion.Element(XName.Get(nombre, "http://tempuri.org/"))?.Value;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
    {
        Url = request.RequestUri!.ToString();
        Accion = request.Headers.TryGetValues("SOAPAction", out var v) ? v.First() : "";
        Cuerpo = request.Content is null ? "" : await request.Content.ReadAsStringAsync(token);
        if (SinRed)
            throw new HttpRequestException("sin red");
        return new HttpResponseMessage(Codigo) { Content = new StringContent(Respuesta, Encoding.UTF8, "text/xml") };
    }
}

internal static class PruebasControladores
{
    private const string Base = "http://servidor:5171";

    private static ServidorFalso Preparar(string? respuesta = null)
    {
        var falso = new ServidorFalso();
        if (respuesta is not null)
            falso.Respuesta = ServidorFalso.Sobre(respuesta);
        ConfigHTTP.Handler = falso;
        ConfigHTTP.BaseUrl = Base;
        ConfigHTTP.Limpiar();
        return falso;
    }

    private static void SesionEmpleado()
    {
        ConfigHTTP.Token = "tok-e";
        ConfigHTTP.Perfil = ConfigHTTP.EMPLEADO;
    }

    public static Ejecutor Crear()
    {
        var e = new Ejecutor();

        e.Caso("loginEmpleado: URL, SOAPAction, sobre y sesión del empleado", async () =>
        {
            var s = Preparar("<r><Estado>OK</Estado><Mensaje>Ingreso de sesión exitoso</Mensaje><Token>tok-e</Token><Perfil>EMPLEADO</Perfil><Nombre>Carla Andrade</Nombre></r>");
            var r = await new LoginController().LoginEmpleadoAsync("cajero", "cajero123");
            Afirmar.Igual(EstadosEnum.OK, r.GetEstado());
            Afirmar.Igual(Base + "/LoginWebService.svc", s.Url);
            Afirmar.Igual("\"http://tempuri.org/ILoginWebService/loginEmpleado\"", s.Accion);
            Afirmar.Igual("loginEmpleado", s.Operacion.Name.LocalName);
            Afirmar.Igual("http://tempuri.org/", s.Operacion.Name.NamespaceName);
            Afirmar.Igual("cajero", s.Parametro("usuario"));
            Afirmar.Igual("cajero123", s.Parametro("clave"));
            Afirmar.Igual("tok-e", ConfigHTTP.Token);
            Afirmar.Falso(ConfigHTTP.EsCliente(), "perfil empleado");
        });

        e.Caso("loginCliente guarda la cuenta de la sesión", async () =>
        {
            var s = Preparar("<r><Estado>OK</Estado><Mensaje>ok</Mensaje><Token>tok-c</Token><Perfil>CLIENTE</Perfil><Nombre>Ana Torres</Nombre></r>");
            await new LoginController().LoginClienteAsync("00100001", "123456");
            Afirmar.Igual("\"http://tempuri.org/ILoginWebService/loginCliente\"", s.Accion);
            Afirmar.Igual("00100001", s.Parametro("cuenta"));
            Afirmar.Verdadero(ConfigHTTP.EsCliente(), "perfil cliente");
            Afirmar.Igual("00100001", ConfigHTTP.Cuenta);
        });

        e.Caso("Login rechazado no abre sesión", async () =>
        {
            Preparar("<r><Estado>ERROR</Estado><Mensaje>Credenciales incorrectas</Mensaje></r>");
            var r = await new LoginController().LoginClienteAsync("00100001", "999999");
            Afirmar.Igual("Credenciales incorrectas", r.GetMensaje());
            Afirmar.Falso(ConfigHTTP.HaySesion(), "sin sesión");
        });

        e.Caso("Sin servidor, SOAP Fault o respuesta que no es XML", async () =>
        {
            var s = Preparar();
            s.SinRed = true;
            Afirmar.Igual(ErroresCliente.SinServidor, (await new LoginController().LoginEmpleadoAsync("cajero", "x")).GetMensaje());
            s.SinRed = false;
            s.Codigo = HttpStatusCode.InternalServerError;
            s.Respuesta = ServidorFalso.Sobre("<s:Fault><faultcode>s:Client</faultcode><faultstring>x</faultstring></s:Fault>");
            Afirmar.Igual(ErroresCliente.FallaServidor, (await new LoginController().LoginEmpleadoAsync("cajero", "x")).GetMensaje());
            s.Codigo = HttpStatusCode.NotFound;
            s.Respuesta = "<html>404";
            Afirmar.Igual(ErroresCliente.RespuestaIncompleta, (await new LoginController().LoginEmpleadoAsync("cajero", "x")).GetMensaje());
        });

        e.Caso("logout envía el token y borra la sesión aunque falle la red", async () =>
        {
            var s = Preparar();
            SesionEmpleado();
            s.SinRed = true;
            await new LoginController().LogoutAsync();
            Afirmar.Igual("\"http://tempuri.org/ILoginWebService/logout\"", s.Accion);
            Afirmar.Igual("tok-e", s.Parametro("token"));
            Afirmar.Falso(ConfigHTTP.HaySesion(), "sesión borrada");
        });

        e.Caso("consultarMovimientos envía token y cuenta", async () =>
        {
            var s = Preparar("<r><Estado>OK</Estado><Mensaje>ok</Mensaje><Cuenta><NumeroCuenta>00100001</NumeroCuenta><Saldo>500</Saldo></Cuenta></r>");
            SesionEmpleado();
            var r = await new MovimientoController().ConsultarAsync("00100001");
            Afirmar.Igual(Base + "/MovimientoWebService.svc", s.Url);
            Afirmar.Igual("\"http://tempuri.org/IMovimientoWebService/consultarMovimientos\"", s.Accion);
            Afirmar.Igual("00100001", s.Parametro("cuenta"));
            Afirmar.Igual(500m, r.Saldo);
        });

        e.Caso("registrarDeposito: importe con dos decimales y sin clave", async () =>
        {
            var s = Preparar("<r><Estado>OK</Estado><Mensaje>Depósito registrado correctamente</Mensaje><NumeroMovimiento>5</NumeroMovimiento><NuevoSaldo>620.50</NuevoSaldo></r>");
            SesionEmpleado();
            var r = await new MovimientoController().DepositoAsync("00100001", 120.5m);
            Afirmar.Igual("\"http://tempuri.org/IMovimientoWebService/registrarDeposito\"", s.Accion);
            Afirmar.Igual("120.50", s.Parametro("importe"));
            Afirmar.Nulo(s.Parametro("clave"), "sin clave");
            Afirmar.Igual(5, r.NumeroMovimiento);
            Afirmar.Igual(620.50m, r.NuevoSaldo);
        });

        e.Caso("registrarRetiro del cliente envía la clave vacía", async () =>
        {
            var s = Preparar();
            ConfigHTTP.Token = "tok-c";
            await new MovimientoController().RetiroAsync("00100001", 40m, "");
            Afirmar.Igual("\"http://tempuri.org/IMovimientoWebService/registrarRetiro\"", s.Accion);
            Afirmar.Igual("", s.Parametro("clave"));
            Afirmar.Igual("40.00", s.Parametro("importe"));
        });

        e.Caso("registrarTransferencia envía las dos cuentas y la clave", async () =>
        {
            var s = Preparar();
            SesionEmpleado();
            await new MovimientoController().TransferenciaAsync("00100002", "00100001", 200m, "654321");
            Afirmar.Igual("\"http://tempuri.org/IMovimientoWebService/registrarTransferencia\"", s.Accion);
            Afirmar.Igual("00100002", s.Parametro("cuentaOrigen"));
            Afirmar.Igual("00100001", s.Parametro("cuentaDestino"));
            Afirmar.Igual("654321", s.Parametro("clave"));
        });

        e.Caso("Operación con token vencido se reconoce como sesión expirada", async () =>
        {
            Preparar("<r><Estado>ERROR</Estado><Mensaje>Token inválido o sesión expirada</Mensaje></r>");
            var r = await new MovimientoController().RetiroAsync("00100001", 1m, "");
            Afirmar.Verdadero(r.SesionExpirada(), "sesión expirada");
        });

        return e;
    }
}
