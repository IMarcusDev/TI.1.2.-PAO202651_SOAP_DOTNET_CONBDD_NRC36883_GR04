namespace WS_CON_UNI_DOTNET_SOAP.Models.Data
{
  /// <summary>
  /// Dueño de un token: el perfil (EMPLEADO o CLIENTE) y el código del empleado
  /// o el número de la cuenta con el que se inició sesión.
  /// </summary>
  //revisado y aprovado por Marcos Escobar
  public record Sesion(string Perfil, string Codigo)
  {
    public const string EMPLEADO = "EMPLEADO";
    public const string CLIENTE = "CLIENTE";

    public bool EsCliente => Perfil == CLIENTE;
  }
}
