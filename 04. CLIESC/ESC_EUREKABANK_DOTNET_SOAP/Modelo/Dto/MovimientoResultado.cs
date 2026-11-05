namespace ec.edu.monster.modelo.dto;

/// <summary>Answer of registrarDeposito / registrarRetiro / registrarTransferencia.</summary>
public record MovimientoResultado(
    bool Ok,
    string Mensaje,
    string NumeroMovimiento = "",
    decimal? NuevoSaldo = null,
    bool SesionExpirada = false)
{
    public static MovimientoResultado Error(string mensaje, bool sesionExpirada = false)
        => new(false, mensaje, SesionExpirada: sesionExpirada);
}
