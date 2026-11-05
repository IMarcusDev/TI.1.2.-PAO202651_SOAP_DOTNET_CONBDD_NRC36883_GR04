using ec.edu.monster.config;
using ec.edu.monster.modelos.dto;
using ec.edu.monster.modelos.soap;
using Microsoft.Extensions.Options;

namespace ec.edu.monster.modelos.servicios;

/// <summary>
/// Client-side access to the SOAP MovimientoWebService. Every operation needs the login token.
/// Only the ERS operations are used (registrar*, consultarMovimientos), never the old
/// deposito / retiro / transferencia with a double amount.
/// </summary>
public class MovimientoService(ITransporte transporte, IOptions<ServidorOptions> opciones)
    : ServicioSoap(transporte, opciones, RutasSoap.ServicioMovimiento, RutasSoap.ContratoMovimiento)
{
    public const string NoSePudoRegistrar = "No se pudo registrar la operación, intente nuevamente";
    public const string NoSePudoConsultar = "No se pudieron consultar los movimientos";

    /// <summary>Deposit into any account (employee only); the server takes the employee from the token.</summary>
    public Task<MovimientoResultado> RegistrarDepositoAsync(string token, string cuenta, decimal importe)
        => RegistrarAsync(Peticion("registrarDeposito")
            .Parametro("token", token)
            .Parametro("cuenta", cuenta)
            .Parametro("importe", importe));

    /// <summary>Withdrawal; clave is the account key for an employee and "" for a Cliente.</summary>
    public Task<MovimientoResultado> RegistrarRetiroAsync(string token, string cuenta, decimal importe, string clave)
        => RegistrarAsync(Peticion("registrarRetiro")
            .Parametro("token", token)
            .Parametro("cuenta", cuenta)
            .Parametro("importe", importe)
            .Parametro("clave", clave));

    /// <summary>Transfer; clave is the source account key for an employee and "" for a Cliente.</summary>
    public Task<MovimientoResultado> RegistrarTransferenciaAsync(string token, string cuentaOrigen,
        string cuentaDestino, decimal importe, string clave)
        => RegistrarAsync(Peticion("registrarTransferencia")
            .Parametro("token", token)
            .Parametro("cuentaOrigen", cuentaOrigen)
            .Parametro("cuentaDestino", cuentaDestino)
            .Parametro("importe", importe)
            .Parametro("clave", clave));

    /// <summary>Account data and movement history, formatted for the view.</summary>
    public async Task<HistorialCuenta> ConsultarMovimientosAsync(string token, string cuenta)
    {
        try
        {
            SoapLectorRespuesta respuesta = await EnviarAsync(Peticion("consultarMovimientos")
                .Parametro("token", token)
                .Parametro("cuenta", cuenta));
            if (respuesta.Estado() is null)
                return HistorialCuenta.Error(RespuestaIncompleta);
            if (!respuesta.EsOk())
            {
                string mensaje = MensajeServidor(respuesta.Texto("mensaje"), NoSePudoConsultar);
                return HistorialCuenta.Error(mensaje, EsTokenInvalido(mensaje));
            }

            string? numero = respuesta.Texto("numeroCuenta");
            var datosCuenta = new CuentaConsultada(
                string.IsNullOrEmpty(numero) ? cuenta : numero,
                respuesta.Texto("titular") ?? "",
                respuesta.Texto("moneda") ?? "",
                Formatos.Decimal(respuesta.Texto("saldo")),
                respuesta.Texto("estadoCuenta") ?? "");

            IEnumerable<MovimientoConsultado> filas = respuesta.Registros("numeroMovimiento")
                .Select(registro => new MovimientoConsultado(
                    Formatos.NumeroMovimiento(Valor(registro, "numeroMovimiento")),
                    Formatos.Fecha(Valor(registro, "fecha")),
                    Valor(registro, "tipoMovimiento"),
                    Valor(registro, "accion"),
                    Formatos.Decimal(Valor(registro, "importe")),
                    Formatos.Referencia(Valor(registro, "cuentaReferencia"))));

            return new HistorialCuenta(true, respuesta.Texto("mensaje") ?? "", datosCuenta, filas);
        }
        catch (Exception e)
        {
            return HistorialCuenta.Error(Mensaje(e));
        }
    }

    private async Task<MovimientoResultado> RegistrarAsync(SoapConstructorPeticion peticion)
    {
        try
        {
            SoapLectorRespuesta respuesta = await EnviarAsync(peticion);
            if (respuesta.Estado() is null)
                return MovimientoResultado.Error(RespuestaIncompleta);
            if (!respuesta.EsOk())
            {
                string mensaje = MensajeServidor(respuesta.Texto("mensaje"), NoSePudoRegistrar);
                return MovimientoResultado.Error(mensaje, EsTokenInvalido(mensaje));
            }

            return new MovimientoResultado(true,
                respuesta.Texto("mensaje") ?? "",
                respuesta.Texto("numeroMovimiento") ?? "",
                Formatos.Decimal(respuesta.Texto("nuevoSaldo")));
        }
        catch (Exception e)
        {
            // Never retried automatically, so a movement is not registered twice.
            return MovimientoResultado.Error(Mensaje(e));
        }
    }

    private static string Valor(IReadOnlyDictionary<string, string> registro, string nombre)
        => registro.TryGetValue(nombre, out string? valor) ? valor : "";
}
