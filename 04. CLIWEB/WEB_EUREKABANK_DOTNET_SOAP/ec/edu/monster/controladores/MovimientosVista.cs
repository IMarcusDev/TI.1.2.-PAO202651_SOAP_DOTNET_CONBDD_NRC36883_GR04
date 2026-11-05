using ec.edu.monster.modelos.dto;

namespace ec.edu.monster.controladores;

/// <summary>Data of the "Consultar movimientos" view.</summary>
public class MovimientosVista
{
    public string? Cuenta { get; init; }
    public string? ErrorCuenta { get; init; }
    public string? Error { get; init; }
    public HistorialCuenta? Historial { get; init; }
    public bool EsCliente { get; init; }
}
