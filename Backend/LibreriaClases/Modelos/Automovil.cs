using LibreriaClases.Enumeraciones;

namespace LibreriaClases.Modelos;

public class Automovil : Vehiculo
{
    private int numeroPuertas;
    public int NumeroPuertas
    {
        get => numeroPuertas;
        set
        {
            if (value < 1 || value > 6) throw new ArgumentException("Se permiten de 1 a 6 puertas.");
            numeroPuertas = value;
        }
    }
    public override TipoVehiculo TipoVehiculo => TipoVehiculo.Automovil;
    public Automovil(string placa, Cliente propietario, int numeroPuertas) : base(placa, propietario)
        => NumeroPuertas = numeroPuertas;
    public override decimal CalcularTarifa(int horas)
    {
        ValidarHoras(horas);
        return decimal.Round(TarifaBaseHora * horas, 2);
    }
    public override string ObtenerDescripcion() => $"{base.ObtenerDescripcion()}, {NumeroPuertas} puertas";
}
