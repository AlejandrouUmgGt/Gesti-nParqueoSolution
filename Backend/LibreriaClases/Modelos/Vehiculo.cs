using LibreriaClases.Enumeraciones;

namespace LibreriaClases.Modelos;

public abstract class Vehiculo
{
    private int id;
    private string placa = "";
    private Cliente propietario;
    private decimal tarifaBaseHora = 10m;

    public int Id { get => id; set => id = Validacion.Id(value); }
    public string Placa { get => placa; set => placa = Validacion.Texto(value, "Placa", 15).ToUpperInvariant(); }
    public Cliente Propietario
    {
        get => propietario;
        set => propietario = value ?? throw new ArgumentException("Se requiere propietario.");
    }
    public abstract TipoVehiculo TipoVehiculo { get; }
    public decimal TarifaBaseHora
    {
        get => tarifaBaseHora;
        set => tarifaBaseHora = Validacion.Precio(value, "Tarifa base por hora");
    }

    protected Vehiculo(string placa, Cliente propietario)
    {
        this.propietario = propietario ?? throw new ArgumentException("Se requiere propietario.");
        Placa = placa;
    }

    protected void ValidarHoras(int horas)
    {
        if (horas < 1) throw new ArgumentException("Se cobra como mínimo una hora.");
    }

    public abstract decimal CalcularTarifa(int horas);
    public virtual string ObtenerDescripcion() => $"{TipoVehiculo}: {Placa}";
    public override string ToString() => ObtenerDescripcion();
}
