//revisado y aprovado por Marcos Escobar
using CONSOLE_EUREKABANK_DOTNET_SOAP.Pruebas;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var suites = new List<(string Titulo, Ejecutor Ejecutor)>
{
    ("Validador de datos y formato", PruebasValidador.Crear()),
    ("Lectura de respuestas (DTO)", PruebasDTO.Crear()),
    ("Controladores con servidor simulado", PruebasControladores.Crear()),
};

int total = 0, fallos = 0;
foreach (var (titulo, ejecutor) in suites)
{
    var (t, f) = await ejecutor.EjecutarAsync(titulo);
    total += t;
    fallos += f;
}

Console.WriteLine($"\nResultado: {total - fallos}/{total} pruebas correctas, {fallos} fallidas.");
return fallos == 0 ? 0 : 1;
