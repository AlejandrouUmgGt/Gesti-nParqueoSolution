namespace LibreriaClases.Modelos;

// Centraliza validaciones pequeñas para no repetir sus mensajes.
internal static class Validacion
{
    internal static string Texto(string? valor, string campo, int maximo)
    {
        string texto = valor?.Trim() ?? "";
        if (texto.Length == 0 || texto.Length > maximo)
            throw new ArgumentException($"{campo} debe tener entre 1 y {maximo} caracteres.");
        return texto;
    }

    internal static int Positivo(int valor, string campo)
    {
        if (valor <= 0) throw new ArgumentException($"{campo} debe ser positivo.");
        return valor;
    }

    internal static decimal Precio(decimal valor, string campo)
    {
        if (valor <= 0 || valor > 99999999.99m || decimal.Round(valor, 2) != valor)
            throw new ArgumentException($"{campo} debe ser positivo, con máximo dos decimales.");
        return valor;
    }

    internal static int Id(int valor)
    {
        // Cero representa un objeto que todavía no se ha guardado.
        if (valor < 0) throw new ArgumentException("El identificador no puede ser negativo.");
        return valor;
    }
}
