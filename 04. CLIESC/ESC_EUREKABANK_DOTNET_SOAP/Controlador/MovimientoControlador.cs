using ec.edu.monster.modelo;
using ec.edu.monster.modelo.dto;

namespace ec.edu.monster.controlador;

/// <summary>Outcome of pressing "Revisar" on a movement screen.</summary>
public enum EstadoRevision
{
    /// <summary>The form has per-field errors; the view shows them.</summary>
    Invalido,

    /// <summary>The user declined the confirmation question.</summary>
    Cancelado,

    /// <summary>The operation could not be done; Mensaje explains why.</summary>
    Error,

    /// <summary>The server accepted the operation; Comprobante is ready to show.</summary>
    Exito,
}

public sealed record ResultadoRevision(EstadoRevision Estado, string? Mensaje = null, Comprobante? Comprobante = null)
{
    public static readonly ResultadoRevision Invalido = new(EstadoRevision.Invalido);
    public static readonly ResultadoRevision Cancelado = new(EstadoRevision.Cancelado);

    public static ResultadoRevision Error(string mensaje) => new(EstadoRevision.Error, mensaje);

    public static ResultadoRevision Exito(Comprobante comprobante)
        => new(EstadoRevision.Exito, null, comprobante);
}

/// <summary>
/// Handles one movement screen (deposit, withdrawal or transfer): validates the form, asks for
/// confirmation, calls the server and builds the receipt. The three screens share this controller;
/// the composition root tells it how to word the question and which service call to make.
/// </summary>
public sealed class MovimientoControlador(
    Sesion sesion,
    Func<MovimientoForm, string> pregunta,
    Func<string, MovimientoForm, Task<MovimientoResultado>> operacion,
    Func<MovimientoForm, MovimientoResultado, Comprobante> comprobante)
{
    public const string SesionInactiva = "La sesión no está activa. Vuelva a iniciar sesión.";

    public string Pregunta(MovimientoForm form) => pregunta(form);

    public async Task<ResultadoRevision> RevisarAsync(MovimientoForm form, Func<string, bool> confirmar)
    {
        if (!form.EsValido)
            return ResultadoRevision.Invalido;

        if (!confirmar(Pregunta(form)))
            return ResultadoRevision.Cancelado;

        if (!sesion.EstaActiva)
            return ResultadoRevision.Error(SesionInactiva);

        MovimientoResultado resultado = await operacion(sesion.Token, form);
        return resultado.Ok
            ? ResultadoRevision.Exito(comprobante(form, resultado))
            : ResultadoRevision.Error(resultado.Mensaje);
    }
}
