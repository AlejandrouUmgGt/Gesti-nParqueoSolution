using System.Data;
using Microsoft.Data.SqlClient;
using LibreriaClases.Modelos;
using LibreriaClases.Enumeraciones;
using LibreriaClases.Logica;

namespace LibreriaClases.Datos;

public class PagoRepositorio
{
    private const string Columnas = MovimientoRepositorio.Columnas + ",p.Id AS p_Id,p.Metodo AS p_Metodo,p.Monto AS p_Monto,p.Fecha AS p_Fecha";
    private const string Tablas = MovimientoRepositorio.Tablas + " JOIN dbo.Pago p ON p.MovimientoId=m.Id";

    private static Pago Leer(SqlDataReader lector) => new Pago(MovimientoRepositorio.Leer(lector),
        (MetodoPago)lector.GetInt32(lector.GetOrdinal("p_Metodo")), lector.GetDecimal(lector.GetOrdinal("p_Monto")),
        lector.GetDateTime(lector.GetOrdinal("p_Fecha"))) { Id = lector.GetInt32(lector.GetOrdinal("p_Id")) };

    public List<Pago> Listar()
    {
        using SqlConnection conexion = Conexion.Abrir();
        using SqlCommand comando = new SqlCommand("SELECT " + Columnas + " FROM " + Tablas + " ORDER BY p.Fecha DESC,p.Id DESC", conexion);
        using SqlDataReader lector = comando.ExecuteReader();
        List<Pago> lista = new List<Pago>();
        while (lector.Read()) lista.Add(Leer(lector));
        return lista;
    }

    public Pago? ObtenerPorId(int id)
    {
        DatosSql.IdGuardado(id);
        using SqlConnection conexion = Conexion.Abrir();
        using SqlCommand comando = DatosSql.Crear(conexion,
            "SELECT " + Columnas + " FROM " + Tablas + " WHERE p.Id=@Id", DatosSql.Entero("@Id", id));
        using SqlDataReader lector = comando.ExecuteReader();
        return lector.Read() ? Leer(lector) : null;
    }

    public int Insertar(Pago objeto)
    {
        // No se puede crear un pago aislado del cierre de su movimiento.
        Pago guardado = new GestorParqueo().RegistrarSalida(objeto.Movimiento.Id, objeto.Metodo, objeto.Monto, objeto.Fecha);
        objeto.Id = guardado.Id;
        objeto.Movimiento = guardado.Movimiento;
        objeto.Fecha = guardado.Fecha;
        return objeto.Id;
    }

    public void Actualizar(Pago objeto)
    {
        Sesion.ExigirAdministrador();
        DatosSql.IdGuardado(objeto.Id);
        // El monto y la fecha forman parte del historial; solo se corrige el método.
        DatosSql.Cambiar(@"UPDATE dbo.Pago SET Metodo=@Metodo
            WHERE Id=@Id AND MovimientoId=@Movimiento AND Monto=@Monto AND Fecha=@Fecha",
            DatosSql.Entero("@Metodo", (int)objeto.Metodo), DatosSql.Entero("@Id", objeto.Id),
            DatosSql.Entero("@Movimiento", objeto.Movimiento.Id), DatosSql.Dinero("@Monto", objeto.Monto), DatosSql.Fecha("@Fecha", objeto.Fecha));
    }

    // Anular un pago revierte también la salida, si el vehículo y espacio están libres.
    public void Eliminar(int id)
    {
        Sesion.ExigirAdministrador();
        Pago pago = ObtenerPorId(id) ?? throw new InvalidOperationException("El pago no existe.");
        string sql = DatosSql.Transaccion(@"
            DECLARE @Bloqueo int;
            SELECT @Bloqueo=Id FROM dbo.Vehiculo WITH (UPDLOCK,HOLDLOCK) WHERE Id=@Vehiculo;
            SELECT @Bloqueo=Id FROM dbo.EspacioParqueo WITH (UPDLOCK,HOLDLOCK) WHERE Id=@Espacio;
            IF NOT EXISTS (SELECT 1 FROM dbo.EspacioParqueo WHERE Id=@Espacio AND Disponible=1)
                THROW 50006,N'No se puede anular: el espacio ya está ocupado.',1;
            IF EXISTS (SELECT 1 FROM dbo.Movimiento WHERE FechaHoraSalida IS NULL AND (VehiculoId=@Vehiculo OR EspacioId=@Espacio))
                THROW 50007,N'No se puede anular: el vehículo o espacio tiene otra entrada activa.',1;
            IF EXISTS (SELECT 1 FROM dbo.Movimiento WHERE Id<>@Movimiento
                AND (VehiculoId=@Vehiculo OR EspacioId=@Espacio)
                AND (FechaHoraSalida IS NULL OR FechaHoraSalida>@Entrada OR FechaHoraEntrada>=@Entrada))
                THROW 50023,N'No se puede anular: existe una visita posterior del vehículo o espacio.',1;
            DELETE FROM dbo.Pago WHERE Id=@Id AND MovimientoId=@Movimiento;
            IF @@ROWCOUNT=0 THROW 50008,N'El pago ya no existe.',1;
            UPDATE dbo.Movimiento SET FechaHoraSalida=NULL WHERE Id=@Movimiento AND VehiculoId=@Vehiculo AND EspacioId=@Espacio AND FechaHoraSalida IS NOT NULL;
            IF @@ROWCOUNT=0 THROW 50009,N'El movimiento ya cambió; actualice la consulta.',1;
            UPDATE dbo.EspacioParqueo SET Disponible=0 WHERE Id=@Espacio;");
        DatosSql.Cambiar(sql, DatosSql.Entero("@Id", id), DatosSql.Entero("@Movimiento", pago.Movimiento.Id),
            DatosSql.Entero("@Vehiculo", pago.Movimiento.Vehiculo.Id), DatosSql.Entero("@Espacio", pago.Movimiento.Espacio.Id),
            DatosSql.Fecha("@Entrada", pago.Movimiento.FechaHoraEntrada));
    }

    public DataTable IngresosPorDia(DateTime desde, DateTime hasta)
    {
        DatosSql.Rango(desde, hasta);
        return DatosSql.Tabla(@"SELECT CAST(Fecha AS date) AS Dia,COUNT(*) AS Pagos,SUM(Monto) AS Total
            FROM dbo.Pago WHERE Fecha>=@Desde AND Fecha<@Hasta
            GROUP BY CAST(Fecha AS date) ORDER BY Dia", DatosSql.Fecha("@Desde", desde), DatosSql.Fecha("@Hasta", hasta));
    }

    public DataTable IngresosPorMes(DateTime desde, DateTime hasta)
    {
        DatosSql.Rango(desde, hasta);
        return DatosSql.Tabla(@"SELECT YEAR(Fecha) AS Anio,MONTH(Fecha) AS Mes,COUNT(*) AS Pagos,SUM(Monto) AS Total
            FROM dbo.Pago WHERE Fecha>=@Desde AND Fecha<@Hasta
            GROUP BY YEAR(Fecha),MONTH(Fecha) ORDER BY Anio,Mes", DatosSql.Fecha("@Desde", desde), DatosSql.Fecha("@Hasta", hasta));
    }
}
