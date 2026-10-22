using System.Data;
using Microsoft.Data.SqlClient;
using LibreriaClases.Modelos;
using LibreriaClases.Logica;

namespace LibreriaClases.Datos;

public class MovimientoRepositorio
{
    internal const string Columnas = VehiculoRepositorio.Columnas + @",
        m.Id AS m_Id,m.FechaHoraEntrada AS m_Entrada,m.FechaHoraSalida AS m_Salida,
        e.Id AS e_Id,e.Numero AS e_Numero,e.Nivel AS e_Nivel,e.Disponible AS e_Disponible";
    internal const string Tablas = VehiculoRepositorio.Tablas
        + " JOIN dbo.Movimiento m ON m.VehiculoId=v.Id JOIN dbo.EspacioParqueo e ON e.Id=m.EspacioId";

    internal static Movimiento Leer(SqlDataReader lector)
    {
        EspacioParqueo espacio = new EspacioParqueo(lector.GetInt32(lector.GetOrdinal("e_Numero")), lector.GetInt32(lector.GetOrdinal("e_Nivel")))
        {
            Id = lector.GetInt32(lector.GetOrdinal("e_Id")),
            Disponible = lector.GetBoolean(lector.GetOrdinal("e_Disponible"))
        };
        return new Movimiento(VehiculoRepositorio.Leer(lector), espacio, lector.GetDateTime(lector.GetOrdinal("m_Entrada")))
        {
            Id = lector.GetInt32(lector.GetOrdinal("m_Id")),
            FechaHoraSalida = lector.IsDBNull(lector.GetOrdinal("m_Salida")) ? null : lector.GetDateTime(lector.GetOrdinal("m_Salida"))
        };
    }

    public List<Movimiento> Listar(bool soloActivos = false)
    {
        using SqlConnection conexion = Conexion.Abrir();
        using SqlCommand comando = DatosSql.Crear(conexion,
            "SELECT " + Columnas + " FROM " + Tablas + " WHERE @Activos=0 OR m.FechaHoraSalida IS NULL ORDER BY m.FechaHoraEntrada DESC,m.Id DESC",
            DatosSql.Bit("@Activos", soloActivos));
        using SqlDataReader lector = comando.ExecuteReader();
        List<Movimiento> lista = new List<Movimiento>();
        while (lector.Read()) lista.Add(Leer(lector));
        return lista;
    }

    public Movimiento? ObtenerPorId(int id)
    {
        DatosSql.IdGuardado(id);
        using SqlConnection conexion = Conexion.Abrir();
        using SqlCommand comando = DatosSql.Crear(conexion,
            "SELECT " + Columnas + " FROM " + Tablas + " WHERE m.Id=@Id", DatosSql.Entero("@Id", id));
        using SqlDataReader lector = comando.ExecuteReader();
        return lector.Read() ? Leer(lector) : null;
    }

    public int Insertar(Movimiento objeto)
    {
        if (!objeto.Activo) throw new ArgumentException("Una entrada nueva no puede tener salida.");
        objeto.Id = new GestorParqueo().RegistrarEntrada(objeto.Vehiculo.Id, objeto.Espacio.Id, objeto.FechaHoraEntrada);
        objeto.FechaHoraEntrada = GestorParqueo.SinFracciones(objeto.FechaHoraEntrada);
        objeto.Espacio.Disponible = false;
        return objeto.Id;
    }

    // Solo se corrige la hora de una entrada aún abierta; el cierre lleva pago.
    public void Actualizar(Movimiento objeto)
    {
        Sesion.ExigirAdministrador();
        DatosSql.IdGuardado(objeto.Id);
        if (!objeto.Activo || objeto.FechaHoraEntrada > DateTime.Now)
            throw new ArgumentException("Solo se puede corregir una entrada activa sin fecha futura.");
        DatosSql.Cambiar(DatosSql.Transaccion(@"
            DECLARE @Bloqueo int;
            SELECT @Bloqueo=Id FROM dbo.Vehiculo WITH (UPDLOCK,HOLDLOCK) WHERE Id=@Vehiculo;
            SELECT @Bloqueo=Id FROM dbo.EspacioParqueo WITH (UPDLOCK,HOLDLOCK) WHERE Id=@Espacio;
            IF EXISTS (SELECT 1 FROM dbo.Movimiento WHERE Id<>@Id AND FechaHoraSalida>@Entrada
                AND (VehiculoId=@Vehiculo OR EspacioId=@Espacio))
                THROW 50019,N'La corrección se solapa con otra visita.',1;
            UPDATE dbo.Movimiento SET FechaHoraEntrada=@Entrada
                WHERE Id=@Id AND VehiculoId=@Vehiculo AND EspacioId=@Espacio AND FechaHoraSalida IS NULL;
            IF @@ROWCOUNT=0 THROW 50020,N'La entrada ya no está activa o sus datos cambiaron.',1;"),
            DatosSql.Fecha("@Entrada", GestorParqueo.SinFracciones(objeto.FechaHoraEntrada)), DatosSql.Entero("@Id", objeto.Id),
            DatosSql.Entero("@Vehiculo", objeto.Vehiculo.Id), DatosSql.Entero("@Espacio", objeto.Espacio.Id));
    }

    // Eliminar equivale a cancelar una entrada sin pago y libera su espacio.
    public void Eliminar(int id)
    {
        Sesion.ExigirAdministrador();
        Movimiento movimiento = ObtenerPorId(id) ?? throw new InvalidOperationException("El movimiento no existe.");
        string sql = DatosSql.Transaccion(@"
            DECLARE @Bloqueo int;
            SELECT @Bloqueo=Id FROM dbo.Vehiculo WITH (UPDLOCK,HOLDLOCK) WHERE Id=@Vehiculo;
            SELECT @Bloqueo=Id FROM dbo.EspacioParqueo WITH (UPDLOCK,HOLDLOCK) WHERE Id=@Espacio;
            DELETE FROM dbo.Movimiento WHERE Id=@Id AND VehiculoId=@Vehiculo AND EspacioId=@Espacio AND FechaHoraSalida IS NULL;
            IF @@ROWCOUNT=0 THROW 50005,N'Solo se puede cancelar una entrada activa sin pago.',1;
            UPDATE dbo.EspacioParqueo SET Disponible=1 WHERE Id=@Espacio;");
        DatosSql.Cambiar(sql, DatosSql.Entero("@Id", id), DatosSql.Entero("@Vehiculo", movimiento.Vehiculo.Id), DatosSql.Entero("@Espacio", movimiento.Espacio.Id));
    }

    public DataTable OcupacionPorHora(DateTime dia)
    {
        // Foto al inicio de cada hora, no suma de todas las visitas de esa hora.
        return DatosSql.Tabla(@"
            SELECT h.Hora,COUNT(m.Id) AS Ocupados,
                (SELECT COUNT(*) FROM dbo.EspacioParqueo) AS Capacidad
            FROM (VALUES (0),(1),(2),(3),(4),(5),(6),(7),(8),(9),(10),(11),
                (12),(13),(14),(15),(16),(17),(18),(19),(20),(21),(22),(23)) h(Hora)
            LEFT JOIN dbo.Movimiento m ON m.FechaHoraEntrada<=DATEADD(hour,h.Hora,@Dia)
                AND (m.FechaHoraSalida IS NULL OR m.FechaHoraSalida>DATEADD(hour,h.Hora,@Dia))
            WHERE DATEADD(hour,h.Hora,@Dia)<=SYSDATETIME()
            GROUP BY h.Hora ORDER BY h.Hora", DatosSql.Fecha("@Dia", dia.Date));
    }

    public DataTable ClientesRecurrentes(DateTime desde, DateTime hasta, int minimoVisitas = 2)
    {
        DatosSql.Rango(desde, hasta);
        if (minimoVisitas < 1) throw new ArgumentException("El mínimo debe ser positivo.");
        return DatosSql.Tabla(@"
            SELECT c.Id,c.Nombres,c.Apellidos,COUNT(m.Id) AS Visitas
            FROM dbo.Cliente c JOIN dbo.Vehiculo v ON v.ClienteId=c.Id
            JOIN dbo.Movimiento m ON m.VehiculoId=v.Id
            WHERE m.FechaHoraEntrada>=@Desde AND m.FechaHoraEntrada<@Hasta
            GROUP BY c.Id,c.Nombres,c.Apellidos HAVING COUNT(m.Id)>=@Minimo
            ORDER BY Visitas DESC,c.Id", DatosSql.Fecha("@Desde", desde), DatosSql.Fecha("@Hasta", hasta), DatosSql.Entero("@Minimo", minimoVisitas));
    }
}
