using Microsoft.Data.SqlClient;

namespace LibreriaClases.Datos;

public static class Conexion
{
    // Cambiar solo esta constante si se utiliza otra instancia de Windows.
    public const string Servidor = @"(localdb)\MSSQLLocalDB";
    private const string Cadena = "Server=" + Servidor
        + ";Database=ParqueoDB;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;";

    public static SqlConnection Abrir()
    {
        SqlConnection conexion = new SqlConnection(Cadena);
        try
        {
            conexion.Open();
            // Estas opciones son necesarias al modificar tablas con índices filtrados.
            using SqlCommand comando = new SqlCommand(@"
                SET ANSI_NULLS ON; SET QUOTED_IDENTIFIER ON; SET ANSI_PADDING ON;
                SET ANSI_WARNINGS ON; SET CONCAT_NULL_YIELDS_NULL ON;
                SET ARITHABORT ON; SET NUMERIC_ROUNDABORT OFF;", conexion);
            comando.ExecuteNonQuery();
            return conexion;
        }
        catch
        {
            conexion.Dispose();
            throw;
        }
    }
}
