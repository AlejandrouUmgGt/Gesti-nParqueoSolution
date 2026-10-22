using LibreriaClases.Enumeraciones;

namespace LibreriaClases.Modelos;

public class Motocicleta : Vehiculo
{
    private int cilindraje;
    private decimal ajuste = 3m;
    public int Cilindraje { get => cilindraje; set => cilindraje = Validacion.Positivo(value, "Cilindraje"); }
    public decimal DescuentoHora
    {
        get => ajuste;
        set
        {
            if (value < 0 || value > 99999999.99m || decimal.Round(value, 2) != value)
                throw new ArgumentException("El ajuste debe ser no negativo y tener máximo dos decimales.");
            ajuste = value;
        }
    }
    public override TipoVehiculo TipoVehiculo => TipoVehiculo.Motocicleta;
    public Motocicleta(string placa, Cliente propietario, int cilindraje) : base(placa, propietario)
        => Cilindraje = cilindraje;
    public override decimal CalcularTarifa(int horas)
    {
        ValidarHoras(horas);
        if (DescuentoHora >= TarifaBaseHora) throw new InvalidOperationException("El descuento debe ser menor a la tarifa base.");
        return decimal.Round((TarifaBaseHora - DescuentoHora) * horas, 2);
    }
    public override string ObtenerDescripcion() => $"{base.ObtenerDescripcion()}, {Cilindraje} cc";
}
