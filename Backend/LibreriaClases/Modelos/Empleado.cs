using LibreriaClases.Enumeraciones;

namespace LibreriaClases.Modelos;

public class Empleado : Persona
{
    private string cargo = "";
    private string usuario = "";
    private string claveHash = "";
    private RolUsuario rol;

    public string Cargo { get => cargo; set => cargo = Validacion.Texto(value, "Cargo", 60); }
    public string Usuario { get => usuario; set => usuario = Validacion.Texto(value, "Usuario", 50); }
    public string ClaveHash
    {
        get => claveHash;
        set
        {
            if (!Logica.Seguridad.EsHashValido(value))
                throw new ArgumentException("La clave debe ser un hash PBKDF2 válido.");
            claveHash = value;
        }
    }
    public bool Activo { get; set; } = true;
    public RolUsuario Rol
    {
        get => rol;
        set
        {
            if (value != RolUsuario.Administrador && value != RolUsuario.Cajero)
                throw new ArgumentException("Rol inválido.");
            rol = value;
        }
    }

    public Empleado(string nombres, string apellidos, string telefono,
        string cargo, string usuario, string claveHash) : base(nombres, apellidos, telefono)
    {
        Cargo = cargo;
        Usuario = usuario;
        ClaveHash = claveHash;
    }

    public override string ObtenerResumen() => $"Empleado: {NombreCompleto}. Cargo: {Cargo}. Rol: {Rol}.";
    public override string ObtenerRol() => Rol.ToString();
}
