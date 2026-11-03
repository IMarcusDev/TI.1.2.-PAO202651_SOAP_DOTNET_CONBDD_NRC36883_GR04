namespace MOV_EUREKABANK_DOTNET_SOAP.Modelos.Data;

public sealed record Movimiento(string Numero, string Fecha, string Tipo, string Accion,
                                decimal? Importe, string CuentaReferencia)
{
    public const string AccionIngreso = "INGRESO";
    public const string AccionSalida = "SALIDA";

    public bool EsIngreso => string.Equals(Accion?.Trim(), AccionIngreso, StringComparison.OrdinalIgnoreCase);

    public bool TieneReferencia => !string.IsNullOrWhiteSpace(CuentaReferencia);
}
