using ec.edu.monster.modelo;
using ec.edu.monster.modelo.dto;

namespace ec.edu.monster.fakes;

/// <summary>Sessions already started, for tests that only need a role.</summary>
public static class Sesiones
{
    public static Sesion Empleado(string usuario = "EMP01") => Crear(usuario, "Ana Pérez", Sesion.RolEmpleado);

    public static Sesion Cliente(string cuenta = "00100001") => Crear(cuenta, "Luis Torres", Sesion.RolCliente);

    public static Sesion Inactiva() => new();

    private static Sesion Crear(string usuario, string nombre, string rol)
    {
        var sesion = new Sesion();
        sesion.Iniciar(usuario, new LoginResultado(true, "ok", "token-123", nombre, rol));
        return sesion;
    }
}
