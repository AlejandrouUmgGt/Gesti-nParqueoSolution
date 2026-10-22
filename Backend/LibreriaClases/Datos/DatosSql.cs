using System.Data;
using Microsoft.Data.SqlClient;

namespace LibreriaClases.Datos;

// Ayudante sencillo: las consultas siguen escritas en cada repositorio.
internal static class DatosSql
{
    internal static SqlParameter Entero(string nombre, int valor) => new SqlParameter(nombre, SqlDbType.Int) { Value = valor };
    internal static SqlParameter Texto(string nombre, string valor, int largo) => new SqlParameter(nombre, SqlDbType.NVarChar, largo) { Value = valor };
    internal static SqlParameter Bit(string nombre, bool valor) => new SqlParameter(nombre, SqlDbType.Bit) { Value = valor };
    internal static SqlParameter Fecha(string nombre, DateTime valor) => new SqlParameter(nombre, SqlDbType.DateTime2) { Value = valor };
    internal static SqlParameter Dinero(string nombre, decimal valor) => new SqlParameter(nombre, SqlDbType.Decimal) { Precision = 10, Scale = 2, Value = valor };
    internal static SqlParameter Nulo(string nombre, SqlDbType tipo) => new SqlParameter(nombre, tipo) { Value = DBNull.Value };

    internal static SqlCommand Crear(SqlConnection conexion, string sql, params SqlParameter[] parametros)
    {
        SqlCommand comando = new SqlCommand(sql, conexion);
        comando.Parameters.AddRange(parametros);
        return comando;
    }

    internal static int Insertar(string sql, params SqlParameter[] parametros)
    {
        using SqlConnection conexion = Conexion.Abrir();
        using SqlCommand comando = Crear(conexion, sql, parametros);
        return Convert.ToInt32(comando.ExecuteScalar());
    }

    internal static void Cambiar(string sql, params SqlParameter[] parametros)
    {
        using SqlConnection conexion = Conexion.Abrir();
        using SqlCommand comando = Crear(conexion, sql, parametros);
        if (comando.ExecuteNonQuery() == 0)
            throw new InvalidOperationException("No se encontró el registro solicitado.");
    }

    internal static DataTable Tabla(string sql, params SqlParameter[] parametros)
    {
        using SqlConnection conexion = Conexion.Abrir();
        using SqlCommand comando = Crear(conexion, sql, parametros);
        using SqlDataAdapter adaptador = new SqlDataAdapter(comando);
        DataTable tabla = new DataTable();
        adaptador.Fill(tabla);
        return tabla;
    }

    // Transacciones en T-SQL: no se necesita agregar otra clase de acceso SQL.
    internal static string Transaccion(string instrucciones) => @"
        SET XACT_ABORT ON;
        BEGIN TRY
            BEGIN TRANSACTION;
            " + instrucciones + @"
            COMMIT TRANSACTION;
        END TRY
        BEGIN CATCH
            IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
            THROW;
        END CATCH;";

    internal static void IdGuardado(int id)
    {
        if (id <= 0) throw new ArgumentException("Se requiere un identificador guardado.");
    }

    internal static void Rango(DateTime desde, DateTime hasta)
    {
        if (hasta <= desde) throw new ArgumentException("La fecha final debe ser posterior a la inicial.");
    }
}
