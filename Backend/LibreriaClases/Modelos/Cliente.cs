namespace LibreriaClases.Modelos;

public class Cliente : Persona
{
    public bool Membresia { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    public Cliente(string nombres, string apellidos, string telefono)
        : base(nombres, apellidos, telefono) { }

    public override string ObtenerResumen() => $"Cliente: {NombreCompleto}. Membresía: {(Membresia ? "Sí" : "No")}.";
    public override string ObtenerRol() => "Cliente";
}
