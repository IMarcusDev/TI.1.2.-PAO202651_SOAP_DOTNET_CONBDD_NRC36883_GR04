namespace MOV_EUREKABANK_DOTNET_SOAP.Vista.Componentes;

/// <summary>Acceso desde código a la paleta de Resources/Styles/Eureka.xaml.</summary>
public static class Colores
{
    public static Color Primario => De("EurekaPrimario");
    public static Color PrimarioSuave => De("EurekaPrimarioSuave");
    public static Color TextoOscuro => De("TextoOscuro");
    public static Color TextoSecundario => De("TextoSecundario");
    public static Color BordeCampo => De("BordeCampo");
    public static Color BordeTarjeta => De("BordeTarjeta");
    public static Color Ingreso => De("Ingreso");
    public static Color IngresoFondo => De("IngresoFondo");
    public static Color Salida => De("Salida");
    public static Color SalidaFondo => De("SalidaFondo");
    public static Color TextoError => De("TextoError");
    public static Color ErrorFondo => De("ErrorFondo");

    private static Color De(string clave)
        => Application.Current?.Resources.TryGetValue(clave, out object? valor) == true && valor is Color color
            ? color
            : Colors.Gray;
}
