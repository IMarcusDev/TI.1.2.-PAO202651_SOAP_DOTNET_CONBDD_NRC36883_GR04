namespace MOV_EUREKABANK_DOTNET_SOAP.Pruebas;

public class FalloPrueba(string mensaje) : Exception(mensaje);

public static class Afirmar
{
    public static void Igual<T>(T esperado, T actual, string contexto = "")
    {
        if (!EqualityComparer<T>.Default.Equals(esperado, actual))
            throw new FalloPrueba($"{contexto} esperado «{esperado}», obtenido «{actual}»".Trim());
    }

    public static void Verdadero(bool condicion, string mensaje)
    {
        if (!condicion)
            throw new FalloPrueba(mensaje);
    }

    public static void Falso(bool condicion, string mensaje) => Verdadero(!condicion, mensaje);

    public static void Nulo(object? valor, string contexto = "")
        => Verdadero(valor is null, $"{contexto} esperado null, obtenido «{valor}»".Trim());

    public static void Contiene(string esperado, string actual)
    {
        if (!actual.Contains(esperado, StringComparison.Ordinal))
            throw new FalloPrueba($"esperado que contenga «{esperado}», obtenido «{actual}»");
    }

    public static void EmpiezaCon(string prefijo, string actual)
    {
        if (!actual.StartsWith(prefijo, StringComparison.Ordinal))
            throw new FalloPrueba($"esperado que empiece con «{prefijo}», obtenido «{actual}»");
    }
}

public class Ejecutor
{
    private readonly List<(string Nombre, Func<Task> Prueba)> _casos = [];

    public void Caso(string nombre, Action prueba) => _casos.Add((nombre, () => { prueba(); return Task.CompletedTask; }));

    public void Caso(string nombre, Func<Task> prueba) => _casos.Add((nombre, prueba));

    public async Task<(int Total, int Fallos)> EjecutarAsync(string titulo)
    {
        Console.WriteLine($"\n== {titulo} ({_casos.Count} casos)");
        int fallos = 0;
        foreach (var (nombre, prueba) in _casos)
        {
            try
            {
                await prueba();
                Console.WriteLine($"  OK    {nombre}");
            }
            catch (Exception ex)
            {
                fallos++;
                string detalle = ex is FalloPrueba ? ex.Message : $"{ex.GetType().Name}: {ex.Message}";
                Console.WriteLine($"  FALLA {nombre}\n        {detalle}");
            }
        }
        return (_casos.Count, fallos);
    }
}
