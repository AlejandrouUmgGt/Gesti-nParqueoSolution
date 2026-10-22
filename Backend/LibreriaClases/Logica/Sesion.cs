using LibreriaClases.Datos;
using LibreriaClases.Enumeraciones;
using LibreriaClases.Modelos;

namespace LibreriaClases.Logica;

public static class Sesion
{
    public static Empleado? EmpleadoActual { get; private set; }
    public static bool EsAdministrador => EmpleadoActual?.Rol == RolUsuario.Administrador;

    public static bool Iniciar(string usuario, string clave)
    {
        Cerrar();
        Empleado? empleado = new EmpleadoRepositorio().ObtenerPorUsuario(usuario);
        if (empleado == null || !empleado.Activo || !Seguridad.VerificarClave(clave, empleado.ClaveHash))
            return false;
        EmpleadoActual = empleado;
        return true;
    }
    public static void Cerrar() => EmpleadoActual = null;
    public static void ExigirSesion()
    {
        if (EmpleadoActual == null || !EmpleadoActual.Activo)
            throw new InvalidOperationException("Debe iniciar sesión.");
    }
    public static void ExigirAdministrador()
    {
        ExigirSesion();
        if (!EsAdministrador) throw new InvalidOperationException("Se requiere rol Administrador.");
    }
}
