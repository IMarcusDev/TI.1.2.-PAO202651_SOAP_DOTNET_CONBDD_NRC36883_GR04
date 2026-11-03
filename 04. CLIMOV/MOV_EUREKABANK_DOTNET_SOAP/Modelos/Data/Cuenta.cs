namespace MOV_EUREKABANK_DOTNET_SOAP.Modelos.Data;

public sealed record Cuenta(string Numero, string Titular, string Moneda, decimal? Saldo, string Estado)
{
    public const string EstadoActivo = "ACTIVO";

    public Cuenta ConSaldo(decimal nuevoSaldo) => this with { Saldo = nuevoSaldo };

    /// <summary>Sin estado informado se asume activa, igual que la etiqueta que se muestra.</summary>
    public bool EstaActiva => string.IsNullOrWhiteSpace(Estado)
        || string.Equals(Estado.Trim(), EstadoActivo, StringComparison.OrdinalIgnoreCase);

    public string EstadoVisible => string.IsNullOrWhiteSpace(Estado) ? EstadoActivo : Estado;
}
