namespace LibreriaClases.Modelos;

public abstract class Persona
{
    private int id;
    private string nombres = "";
    private string apellidos = "";
    private string telefono = "";

    public int Id { get => id; set => id = Validacion.Id(value); }
    public string Nombres { get => nombres; set => nombres = Validacion.Texto(value, "Nombres", 80); }
    public string Apellidos { get => apellidos; set => apellidos = Validacion.Texto(value, "Apellidos", 80); }
    public string Telefono { get => telefono; set => telefono = Validacion.Texto(value, "Teléfono", 25); }
    public string NombreCompleto => $"{Nombres} {Apellidos}";

    protected Persona(string nombres, string apellidos, string telefono)
    {
        Nombres = nombres;
        Apellidos = apellidos;
        Telefono = telefono;
    }

    // Cada hija explica sus datos sin que el consumidor conozca su clase.
    public abstract string ObtenerResumen();
    public virtual string ObtenerRol() => "Persona";
    public override string ToString() => NombreCompleto;
}
