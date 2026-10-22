namespace LibreriaClases.Modelos;

public class Movimiento
{
    private int id;
    private Vehiculo vehiculo;
    private EspacioParqueo espacio;
    public int Id { get => id; set => id = Validacion.Id(value); }
    public Vehiculo Vehiculo { get => vehiculo; set => vehiculo = value ?? throw new ArgumentException("Se requiere vehículo."); }
    public EspacioParqueo Espacio { get => espacio; set => espacio = value ?? throw new ArgumentException("Se requiere espacio."); }
    public DateTime FechaHoraEntrada { get; set; }
    public DateTime? FechaHoraSalida { get; set; }
    public TimeSpan Duracion => CalcularDuracion(FechaHoraSalida ?? DateTime.Now);
    public bool Activo => FechaHoraSalida == null;
    public Movimiento(Vehiculo vehiculo, EspacioParqueo espacio, DateTime entrada)
    {
        this.vehiculo = vehiculo ?? throw new ArgumentException("Se requiere vehículo.");
        this.espacio = espacio ?? throw new ArgumentException("Se requiere espacio.");
        FechaHoraEntrada = entrada;
    }
    public TimeSpan CalcularDuracion(DateTime salida)
    {
        if (salida < FechaHoraEntrada) throw new ArgumentException("La salida no puede preceder a la entrada.");
        return salida - FechaHoraEntrada;
    }
}
