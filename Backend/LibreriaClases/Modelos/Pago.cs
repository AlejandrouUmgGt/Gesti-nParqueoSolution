using LibreriaClases.Enumeraciones;

namespace LibreriaClases.Modelos;

public class Pago
{
    private int id;
    private Movimiento movimiento;
    private MetodoPago metodo;
    private decimal monto;
    public int Id { get => id; set => id = Validacion.Id(value); }
    public Movimiento Movimiento { get => movimiento; set => movimiento = value ?? throw new ArgumentException("Se requiere movimiento."); }
    public MetodoPago Metodo
    {
        get => metodo;
        set
        {
            if (value != MetodoPago.Efectivo && value != MetodoPago.Tarjeta) throw new ArgumentException("Método inválido.");
            metodo = value;
        }
    }
    public decimal Monto { get => monto; set => monto = Validacion.Precio(value, "Monto"); }
    public DateTime Fecha { get; set; }
    public Pago(Movimiento movimiento, MetodoPago metodo, decimal monto, DateTime fecha)
    {
        this.movimiento = movimiento ?? throw new ArgumentException("Se requiere movimiento.");
        Metodo = metodo;
        Monto = monto;
        Fecha = fecha;
    }
}
