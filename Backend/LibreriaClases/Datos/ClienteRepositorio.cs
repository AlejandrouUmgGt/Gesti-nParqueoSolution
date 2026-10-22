using Microsoft.Data.SqlClient;
using LibreriaClases.Modelos;
using LibreriaClases.Enumeraciones;
using LibreriaClases.Logica;

namespace LibreriaClases.Datos;

public class ClienteRepositorio
{
    private const string Columnas = "Id,Nombres,Apellidos,Telefono,Membresia,FechaRegistro";

    private static Cliente Leer(SqlDataReader lector) => new Cliente(lector.GetString(lector.GetOrdinal("Nombres")), lector.GetString(lector.GetOrdinal("Apellidos")), lector.GetString(lector.GetOrdinal("Telefono")))
        {
            Id = lector.GetInt32(lector.GetOrdinal("Id")),
            Membresia = lector.GetBoolean(lector.GetOrdinal("Membresia")),
            FechaRegistro = lector.GetDateTime(lector.GetOrdinal("FechaRegistro"))
        };

    public List<Cliente> Listar(string busqueda = "")
    {
        using SqlConnection conexion = Conexion.Abrir();
        using SqlCommand comando = DatosSql.Crear(conexion,
            "SELECT " + Columnas + " FROM dbo.Cliente WHERE Nombres LIKE @Busqueda OR Apellidos LIKE @Busqueda OR Telefono LIKE @Busqueda ORDER BY Apellidos,Nombres,Id",
            DatosSql.Texto("@Busqueda", "%" + busqueda.Trim() + "%", 200));
        using SqlDataReader lector = comando.ExecuteReader();
        List<Cliente> lista = new List<Cliente>();
        while (lector.Read()) lista.Add(Leer(lector));
        return lista;
    }

    public Cliente? ObtenerPorId(int id)
    {
        DatosSql.IdGuardado(id);
        using SqlConnection conexion = Conexion.Abrir();
        using SqlCommand comando = DatosSql.Crear(conexion,
            "SELECT " + Columnas + " FROM dbo.Cliente WHERE Id=@Id", DatosSql.Entero("@Id", id));
        using SqlDataReader lector = comando.ExecuteReader();
        return lector.Read() ? Leer(lector) : null;
    }

    private static SqlParameter[] Parametros(Cliente objeto) => new SqlParameter[]
    {
            DatosSql.Texto("@Nombres", objeto.Nombres, 80),
            DatosSql.Texto("@Apellidos", objeto.Apellidos, 80),
            DatosSql.Texto("@Telefono", objeto.Telefono, 25),
            DatosSql.Bit("@Membresia", objeto.Membresia),
            DatosSql.Fecha("@FechaRegistro", objeto.FechaRegistro)
    };

    public int Insertar(Cliente objeto)
    {
        Sesion.ExigirAdministrador();
        objeto.Id = DatosSql.Insertar("INSERT dbo.Cliente (Nombres,Apellidos,Telefono,Membresia,FechaRegistro) VALUES (@Nombres,@Apellidos,@Telefono,@Membresia,@FechaRegistro); SELECT CAST(SCOPE_IDENTITY() AS int);", Parametros(objeto));
        return objeto.Id;
    }

    public void Actualizar(Cliente objeto)
    {
        Sesion.ExigirAdministrador();
        DatosSql.IdGuardado(objeto.Id);
        using SqlConnection conexion = Conexion.Abrir();
        using SqlCommand comando = DatosSql.Crear(conexion,
            "UPDATE dbo.Cliente SET Nombres=@Nombres,Apellidos=@Apellidos,Telefono=@Telefono,Membresia=@Membresia,FechaRegistro=@FechaRegistro WHERE Id=@Id", Parametros(objeto));
        comando.Parameters.Add(DatosSql.Entero("@Id", objeto.Id));
        if (comando.ExecuteNonQuery() == 0) throw new InvalidOperationException("El registro ya no existe.");
    }

    public void Eliminar(int id)
    {
        Sesion.ExigirAdministrador();
        DatosSql.IdGuardado(id);
        DatosSql.Cambiar("DELETE FROM dbo.Cliente WHERE Id=@Id", DatosSql.Entero("@Id", id));
    }
}
