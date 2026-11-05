using ec.edu.monster.modelo;
using ec.edu.monster.modelo.dto;
using ec.edu.monster.modelo.servicios;

namespace ec.edu.monster.controlador;

/// <summary>
/// Handles the "Consultar movimientos" screen: validates the account, asks the server for the
/// history and returns it already formatted. The view only renders the answer.
/// </summary>
public sealed class MovimientosControlador(Sesion sesion, MovimientoServicio servicio)
{
    public const string CuentaVacia = "Ingrese el número de cuenta";
    public const string SesionInactiva = "La sesión no está activa. Vuelva a iniciar sesión.";

    /// <summary>
    /// Account history for the logged-in session. A Cliente always sends their own account, fixed
    /// by the screen; the server also ties the token to it.
    /// </summary>
    public async Task<HistorialCuenta> ConsultarAsync(string cuenta)
    {
        string numero = (cuenta ?? "").Trim();
        if (numero.Length == 0)
            return HistorialCuenta.Error(CuentaVacia);
        if (!sesion.EstaActiva)
            return HistorialCuenta.Error(SesionInactiva);

        return await servicio.ConsultarMovimientosAsync(sesion.Token, numero);
    }
}
