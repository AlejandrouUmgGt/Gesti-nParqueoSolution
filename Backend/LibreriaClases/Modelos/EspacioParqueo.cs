namespace LibreriaClases.Modelos;

public class EspacioParqueo
{
    private int id;
    private int numero;
    private int nivel;
    public int Id { get => id; set => id = Validacion.Id(value); }
    public int Numero { get => numero; set => numero = Validacion.Positivo(value, "Número"); }
    public int Nivel { get => nivel; set => nivel = Validacion.Positivo(value, "Nivel"); }
    public bool Disponible { get; set; } = true;
    public EspacioParqueo(int numero, int nivel) { Numero = numero; Nivel = nivel; }
    public override string ToString() => $"Nivel {Nivel}, espacio {Numero}";
}
