using Microsoft.Data.SqlClient;
using LibreriaClases.Modelos;
using LibreriaClases.Logica;

namespace LibreriaClases.Datos;

public class EspacioParqueoRepositorio
{
    internal static EspacioParqueo Leer(SqlDataReader lector) => new EspacioParqueo(
        lector.GetInt32(lector.GetOrdinal("Numero")), lector.GetInt32(lector.GetOrdinal("Nivel")))
    {
        Id = lector.GetInt32(lector.GetOrdinal("Id")),
        Disponible = lector.GetBoolean(lector.GetOrdinal("Disponible"))
    };

    public List<EspacioParqueo> Listar(bool soloLibres = false)
    {
        using SqlConnection conexion = Conexion.Abrir();
        using SqlCommand comando = DatosSql.Crear(conexion, @"
            SELECT e.Id,e.Numero,e.Nivel,e.Disponible FROM dbo.EspacioParqueo e
            WHERE @SoloLibres=0 OR (e.Disponible=1 AND NOT EXISTS
                (SELECT 1 FROM dbo.Movimiento m WHERE m.EspacioId=e.Id AND m.FechaHoraSalida IS NULL))
            ORDER BY e.Nivel,e.Numero", DatosSql.Bit("@SoloLibres", soloLibres));
        using SqlDataReader lector = comando.ExecuteReader();
        List<EspacioParqueo> lista = new List<EspacioParqueo>();
        while (lector.Read()) lista.Add(Leer(lector));
        return lista;
    }

    public EspacioParqueo? ObtenerPorId(int id)
    {
        DatosSql.IdGuardado(id);
        using SqlConnection conexion = Conexion.Abrir();
        using SqlCommand comando = DatosSql.Crear(conexion,
            "SELECT Id,Numero,Nivel,Disponible FROM dbo.EspacioParqueo WHERE Id=@Id", DatosSql.Entero("@Id", id));
        using SqlDataReader lector = comando.ExecuteReader();
        return lector.Read() ? Leer(lector) : null;
    }

    public int Insertar(EspacioParqueo objeto)
    {
        Sesion.ExigirAdministrador();
        if (!objeto.Disponible) throw new ArgumentException("Un espacio nuevo debe estar disponible.");
        objeto.Id = DatosSql.Insertar(@"INSERT dbo.EspacioParqueo (Numero,Nivel) VALUES (@Numero,@Nivel);
            SELECT CAST(SCOPE_IDENTITY() AS int);", DatosSql.Entero("@Numero", objeto.Numero), DatosSql.Entero("@Nivel", objeto.Nivel));
        return objeto.Id;
    }

    public void Actualizar(EspacioParqueo objeto)
    {
        Sesion.ExigirAdministrador();
        DatosSql.IdGuardado(objeto.Id);
        // La disponibilidad solo cambia con una entrada, salida o cancelación.
        DatosSql.Cambiar(@"UPDATE dbo.EspacioParqueo SET Numero=@Numero,Nivel=@Nivel WHERE Id=@Id",
            DatosSql.Entero("@Numero", objeto.Numero), DatosSql.Entero("@Nivel", objeto.Nivel), DatosSql.Entero("@Id", objeto.Id));
    }

    public void Eliminar(int id)
    {
        Sesion.ExigirAdministrador();
        DatosSql.IdGuardado(id);
        DatosSql.Cambiar("DELETE FROM dbo.EspacioParqueo WHERE Id=@Id", DatosSql.Entero("@Id", id));
    }
}
