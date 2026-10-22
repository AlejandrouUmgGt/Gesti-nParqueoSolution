using LibreriaClases.Enumeraciones;

namespace LibreriaClases.Modelos;

public class Tarifa
{
    private int id;
    private TipoVehiculo tipoVehiculo;
    private decimal precioHora;
    private decimal precioDia;
    private decimal precioMensualidad;
    public int Id { get => id; set => id = Validacion.Id(value); }
    public TipoVehiculo TipoVehiculo
    {
        get => tipoVehiculo;
        set
        {
            if (value < TipoVehiculo.Automovil || value > TipoVehiculo.Camion)
                throw new ArgumentException("Tipo de vehículo inválido.");
            tipoVehiculo = value;
        }
    }
    public decimal PrecioHora { get => precioHora; set => precioHora = Validacion.Precio(value, "Precio por hora"); }
    public decimal PrecioDia { get => precioDia; set => precioDia = Validacion.Precio(value, "Precio por día"); }
    public decimal PrecioMensualidad { get => precioMensualidad; set => precioMensualidad = Validacion.Precio(value, "Mensualidad"); }
    public Tarifa(TipoVehiculo tipoVehiculo, decimal precioHora, decimal precioDia, decimal precioMensualidad)
    {
        TipoVehiculo = tipoVehiculo;
        PrecioHora = precioHora;
        PrecioDia = precioDia;
        PrecioMensualidad = precioMensualidad;
    }
}
