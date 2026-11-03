using MOV_EUREKABANK_DOTNET_SOAP.Controlador;
using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Data;
using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Enums;
using MOV_EUREKABANK_DOTNET_SOAP.Modelos.Interfaces;
using MOV_EUREKABANK_DOTNET_SOAP.Servicios;

namespace MOV_EUREKABANK_DOTNET_SOAP.Pruebas;

public static class PruebasOperacionControlador
{
    /// <summary>Repositorio en memoria que recuerda la última solicitud y responde lo que se le indique.</summary>
    private sealed class RepositorioFalso : IMovimientoRepositorio
    {
        public SolicitudOperacion? Ultima { get; private set; }
        public Resultado<Comprobante>? Respuesta { get; set; }

        public Task<Resultado<ConsultaMovimientos>> ConsultarMovimientosAsync(string token, string cuenta)
        {
            var datos = new Cuenta(cuenta, "Ana Pérez", "USD", 500.00m, "ACTIVO");
            return Task.FromResult(Resultado<ConsultaMovimientos>.Ok(new ConsultaMovimientos(datos, []), "ok"));
        }

        public Task<Resultado<Comprobante>> RegistrarAsync(string token, SolicitudOperacion solicitud)
        {
            Ultima = solicitud;
            return Task.FromResult(Respuesta ?? Resultado<Comprobante>.Ok(new Comprobante(solicitud, "7", 400.00m, DateTime.Now), "ok"));
        }
    }

    private static SesionServicio Sesion(Perfil perfil)
    {
        var sesion = new SesionServicio();
        sesion.Iniciar(new Sesion("tok", perfil, "Usuario", perfil == Perfil.Cliente ? "00100001" : null));
        return sesion;
    }

    public static Ejecutor Crear()
    {
        var e = new Ejecutor();

        e.Caso("Cliente: solo puede retirar o transferir, y sin clave", () =>
        {
            var controlador = new OperacionControlador(new RepositorioFalso(), Sesion(Perfil.Cliente));

            Afirmar.Falso(controlador.TiposDisponibles().Contains(TipoOperacion.Deposito), "el cliente no deposita");
            Afirmar.Falso(controlador.PideCuenta, "no pide cuenta");
            Afirmar.Falso(controlador.PideClave(TipoOperacion.Retiro), "no pide clave");
            Afirmar.Igual(Campo.Ninguno, controlador.Validar(TipoOperacion.Deposito, "", "", "10", "").Campo);
        });
        e.Caso("Cliente: opera sobre la cuenta de su sesión con clave vacía", async () =>
        {
            var repositorio = new RepositorioFalso();
            SesionServicio sesion = Sesion(Perfil.Cliente);
            var controlador = new OperacionControlador(repositorio, sesion);
            await controlador.CargarCuentaClienteAsync();

            SolicitudOperacion solicitud = controlador.Preparar(TipoOperacion.Retiro, "99999999", "", "100", "123456");
            Afirmar.Verdadero((await controlador.RegistrarAsync(solicitud)).Exito, "debía registrar");

            Afirmar.Igual("00100001", repositorio.Ultima!.Cuenta);
            Afirmar.Igual("", repositorio.Ultima.Clave);
            Afirmar.Igual(400.00m, sesion.CuentaCliente!.Saldo);
        });
        e.Caso("Empleado: debe ingresar la clave en retiro y transferencia", () =>
        {
            var controlador = new OperacionControlador(new RepositorioFalso(), Sesion(Perfil.Empleado));

            Afirmar.Verdadero(controlador.PideClave(TipoOperacion.Retiro), "pide clave en retiro");
            Afirmar.Falso(controlador.PideClave(TipoOperacion.Deposito), "no pide clave en depósito");
            Afirmar.Igual(Campo.Clave, controlador.Validar(TipoOperacion.Retiro, "00100001", "", "50", "").Campo);
            Afirmar.Verdadero(controlador.Validar(TipoOperacion.Deposito, "00100001", "", "50", "").Valido, "depósito válido");
        });
        e.Caso("Transferencia: rechaza cuentas iguales", () =>
        {
            var controlador = new OperacionControlador(new RepositorioFalso(), Sesion(Perfil.Empleado));

            ValidacionResultado resultado = controlador.Validar(TipoOperacion.Transferencia, "00100001", "00100001", "10", "123456");

            Afirmar.Igual(Campo.Destino, resultado.Campo);
            Afirmar.Igual("La cuenta de origen y la de destino deben ser diferentes", resultado.Mensaje);
        });
        e.Caso("Registrar con token inválido borra la sesión", async () =>
        {
            var repositorio = new RepositorioFalso { Respuesta = Resultado<Comprobante>.Expirada("expiró") };
            SesionServicio sesion = Sesion(Perfil.Empleado);
            var controlador = new OperacionControlador(repositorio, sesion);

            SolicitudOperacion solicitud = controlador.Preparar(TipoOperacion.Deposito, "00100001", "", "10", "");
            Afirmar.Verdadero((await controlador.RegistrarAsync(solicitud)).SesionExpirada, "sesión expirada");
            Afirmar.Nulo(sesion.Sesion);
        });

        return e;
    }
}
