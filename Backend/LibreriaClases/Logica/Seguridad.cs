using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace LibreriaClases.Logica;

public static class Seguridad
{
    private const int Iteraciones = 210000;

    public static string CrearHash(string clave)
    {
        if (string.IsNullOrWhiteSpace(clave) || clave.Length < 8 || clave.Length > 200)
            throw new ArgumentException("La contraseña debe tener entre 8 y 200 caracteres.");
        byte[] sal = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(clave), sal,
            Iteraciones, HashAlgorithmName.SHA256, 32);
        return $"PBKDF2-SHA256${Iteraciones}${Convert.ToBase64String(sal)}${Convert.ToBase64String(hash)}";
    }

    public static bool EsHashValido(string? texto)
    {
        if (string.IsNullOrEmpty(texto) || texto.Length > 200) return false;
        string[] partes = texto.Split('$');
        if (partes.Length != 4 || partes[0] != "PBKDF2-SHA256") return false;
        if (!int.TryParse(partes[1], NumberStyles.None, CultureInfo.InvariantCulture, out int vueltas)
            || vueltas < Iteraciones || vueltas > 1000000) return false;
        try
        {
            return Convert.FromBase64String(partes[2]).Length == 16
                && Convert.FromBase64String(partes[3]).Length == 32;
        }
        catch (FormatException) { return false; }
    }

    public static bool VerificarClave(string? clave, string? texto)
    {
        if (clave == null || clave.Length > 200 || !EsHashValido(texto)) return false;
        string[] partes = texto!.Split('$');
        int vueltas = int.Parse(partes[1], CultureInfo.InvariantCulture);
        byte[] esperado = Convert.FromBase64String(partes[3]);
        byte[] obtenido = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(clave),
            Convert.FromBase64String(partes[2]), vueltas, HashAlgorithmName.SHA256, 32);
        // Una comparación de tiempo fijo evita revelar diferencias byte a byte.
        return CryptographicOperations.FixedTimeEquals(esperado, obtenido);
    }
}
