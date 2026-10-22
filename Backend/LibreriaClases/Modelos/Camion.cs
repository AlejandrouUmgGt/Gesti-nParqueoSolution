using LibreriaClases.Enumeraciones;

namespace LibreriaClases.Modelos;

public class Camion : Vehiculo
{
    private decimal capacidadToneladas;
    private decimal ajuste = 5m;
    public decimal CapacidadToneladas { get => capacidadToneladas; set => capacidadToneladas = (value > 0 && value <= 999999.99m && decimal.Round(value, 2) == value ? value : throw new ArgumentException("Capacidad inválida.")); }
    public decimal RecargoHora
    {
        get => ajuste;
        set
        {
            if (value < 0 || value > 99999999.99m || decimal.Round(value, 2) != value)
                throw new ArgumentException("El ajuste debe ser no negativo y tener máximo dos decimales.");
            ajuste = value;
        }
    }
    public override TipoVehiculo TipoVehiculo => TipoVehiculo.Camion;
    public Camion(string placa, Cliente propietario, decimal capacidadToneladas) : base(placa, propietario)
        => CapacidadToneladas = capacidadToneladas;
    public override decimal CalcularTarifa(int horas)
    {
        ValidarHoras(horas);
        
        return decimal.Round((TarifaBaseHora + RecargoHora) * horas, 2);
    }
    public override string ObtenerDescripcion() => $"{base.ObtenerDescripcion()}, {CapacidadToneladas} toneladas";
}
