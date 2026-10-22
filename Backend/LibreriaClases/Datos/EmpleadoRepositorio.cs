using Microsoft.Data.SqlClient;
using LibreriaClases.Modelos;
using LibreriaClases.Enumeraciones;
using LibreriaClases.Logica;

namespace LibreriaClases.Datos;

public class EmpleadoRepositorio
{
    private const string Columnas = "Id,Nombres,Apellidos,Telefono,Cargo,Usuario,ClaveHash,Activo,Rol";

    private static Empleado Leer(SqlDataReader lector) => new Empleado(lector.GetString(lector.GetOrdinal("Nombres")), lector.GetString(lector.GetOrdinal("Apellidos")), lector.GetString(lector.GetOrdinal("Telefono")),
            lector.GetString(lector.GetOrdinal("Cargo")), lector.GetString(lector.GetOrdinal("Usuario")), lector.GetString(lector.GetOrdinal("ClaveHash")))
        {
            Id = lector.GetInt32(lector.GetOrdinal("Id")),
            Activo = lector.GetBoolean(lector.GetOrdinal("Activo")),
            Rol = (RolUsuario)lector.GetInt32(lector.GetOrdinal("Rol"))
        };

    public List<Empleado> Listar(string busqueda = "")
    {
        using SqlConnection conexion = Conexion.Abrir();
        using SqlCommand comando = DatosSql.Crear(conexion,
            "SELECT " + Columnas + " FROM dbo.Empleado WHERE Nombres LIKE @Busqueda OR Apellidos LIKE @Busqueda OR Usuario LIKE @Busqueda ORDER BY Apellidos,Nombres,Id",
            DatosSql.Texto("@Busqueda", "%" + busqueda.Trim() + "%", 200));
        using SqlDataReader lector = comando.ExecuteReader();
        List<Empleado> lista = new List<Empleado>();
        while (lector.Read()) lista.Add(Leer(lector));
        return lista;
    }

    public Empleado? ObtenerPorId(int id)
    {
        DatosSql.IdGuardado(id);
        using SqlConnection conexion = Conexion.Abrir();
        using SqlCommand comando = DatosSql.Crear(conexion,
            "SELECT " + Columnas + " FROM dbo.Empleado WHERE Id=@Id", DatosSql.Entero("@Id", id));
        using SqlDataReader lector = comando.ExecuteReader();
        return lector.Read() ? Leer(lector) : null;
    }

    public Empleado? ObtenerPorUsuario(string usuario)
    {
        if (string.IsNullOrWhiteSpace(usuario) || usuario.Trim().Length > 50) return null;
        using SqlConnection conexion = Conexion.Abrir();
        using SqlCommand comando = DatosSql.Crear(conexion, "SELECT " + Columnas + " FROM dbo.Empleado WHERE Usuario=@Usuario",
            DatosSql.Texto("@Usuario", usuario.Trim(), 50));
        using SqlDataReader lector = comando.ExecuteReader();
        return lector.Read() ? Leer(lector) : null;
    }

    private static SqlParameter[] Parametros(Empleado objeto) => new SqlParameter[]
    {
            DatosSql.Texto("@Nombres", objeto.Nombres, 80),
            DatosSql.Texto("@Apellidos", objeto.Apellidos, 80),
            DatosSql.Texto("@Telefono", objeto.Telefono, 25),
            DatosSql.Texto("@Cargo", objeto.Cargo, 60),
            DatosSql.Texto("@Usuario", objeto.Usuario, 50),
            new SqlParameter("@ClaveHash", System.Data.SqlDbType.VarChar, 200) { Value = objeto.ClaveHash },
            DatosSql.Bit("@Activo", objeto.Activo),
            DatosSql.Entero("@Rol", (int)objeto.Rol)
    };

    public int Insertar(Empleado objeto)
    {
        Sesion.ExigirAdministrador();
        objeto.Id = DatosSql.Insertar("INSERT dbo.Empleado (Nombres,Apellidos,Telefono,Cargo,Usuario,ClaveHash,Activo,Rol) VALUES (@Nombres,@Apellidos,@Telefono,@Cargo,@Usuario,@ClaveHash,@Activo,@Rol); SELECT CAST(SCOPE_IDENTITY() AS int);", Parametros(objeto));
        return objeto.Id;
    }

    public void Actualizar(Empleado objeto)
    {
        Sesion.ExigirAdministrador();
        DatosSql.IdGuardado(objeto.Id);
        using SqlConnection conexion = Conexion.Abrir();
        using SqlCommand comando = DatosSql.Crear(conexion,
            "UPDATE dbo.Empleado SET Nombres=@Nombres,Apellidos=@Apellidos,Telefono=@Telefono,Cargo=@Cargo,Usuario=@Usuario,ClaveHash=@ClaveHash,Activo=@Activo,Rol=@Rol WHERE Id=@Id", Parametros(objeto));
        comando.Parameters.Add(DatosSql.Entero("@Id", objeto.Id));
        if (comando.ExecuteNonQuery() == 0) throw new InvalidOperationException("El registro ya no existe.");
    }

    public void Eliminar(int id)
    {
        Sesion.ExigirAdministrador();
        DatosSql.IdGuardado(id);
        DatosSql.Cambiar("DELETE FROM dbo.Empleado WHERE Id=@Id", DatosSql.Entero("@Id", id));
    }
}
