namespace MOV_EUREKABANK_DOTNET_SOAP.Modelos.Data;

/// <param name="Movimientos">Movimientos en el orden en que los envía el servidor.</param>
public sealed record ConsultaMovimientos(Cuenta Cuenta, IReadOnlyList<Movimiento> Movimientos)
{
    /// <summary>
    /// Movimientos del más reciente al más antiguo: es el orden en que se muestran.
    /// El número de movimiento es el ID global del servidor, que crece con cada registro, así que el
    /// mayor es el más reciente. No depende del orden de la respuesta; los números que no se pueden
    /// leer quedan al final, en el orden inverso al de llegada.
    /// </summary>
    public IReadOnlyList<Movimiento> MovimientosRecientes { get; } =
        Movimientos.Reverse().OrderByDescending(m => long.TryParse(m.Numero?.Trim(), out long n) ? n : long.MinValue).ToList();
}
