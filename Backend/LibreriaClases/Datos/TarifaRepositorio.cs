using Microsoft.Data.SqlClient;
using LibreriaClases.Modelos;
using LibreriaClases.Enumeraciones;
using LibreriaClases.Logica;

namespace LibreriaClases.Datos;

public class TarifaRepositorio
{
    private static Tarifa Leer(SqlDataReader lector) => new Tarifa(
        (TipoVehiculo)lector.GetInt32(1), lector.GetDecimal(2), lector.GetDecimal(3), lector.GetDecimal(4)) { Id = lector.GetInt32(0) };

    public List<Tarifa> Listar()
    {
        using SqlConnection conexion = Conexion.Abrir();
        using SqlCommand comando = new SqlCommand("SELECT Id,TipoVehiculo,PrecioHora,PrecioDia,PrecioMensualidad FROM dbo.Tarifa ORDER BY TipoVehiculo", conexion);
        using SqlDataReader lector = comando.ExecuteReader();
        List<Tarifa> lista = new List<Tarifa>();
        while (lector.Read()) lista.Add(Leer(lector));
        return lista;
    }

    public Tarifa? ObtenerPorId(int id)
    {
        DatosSql.IdGuardado(id);
        using SqlConnection conexion = Conexion.Abrir();
        using SqlCommand comando = DatosSql.Crear(conexion,
            "SELECT Id,TipoVehiculo,PrecioHora,PrecioDia,PrecioMensualidad FROM dbo.Tarifa WHERE Id=@Id", DatosSql.Entero("@Id", id));
        using SqlDataReader lector = comando.ExecuteReader();
        return lector.Read() ? Leer(lector) : null;
    }

    public Tarifa? ObtenerPorTipo(TipoVehiculo tipo)
    {
        using SqlConnection conexion = Conexion.Abrir();
        using SqlCommand comando = DatosSql.Crear(conexion,
            "SELECT Id,TipoVehiculo,PrecioHora,PrecioDia,PrecioMensualidad FROM dbo.Tarifa WHERE TipoVehiculo=@Tipo", DatosSql.Entero("@Tipo", (int)tipo));
        using SqlDataReader lector = comando.ExecuteReader();
        return lector.Read() ? Leer(lector) : null;
    }

    // El precio de moto no debe superar la base; el de camión no debe ser inferior.
    private const string ComprobarOrden = @"
        IF EXISTS (SELECT 1 FROM dbo.Tarifa a JOIN dbo.Tarifa t ON a.TipoVehiculo=0
            WHERE (t.TipoVehiculo=1 AND t.PrecioHora>a.PrecioHora)
               OR (t.TipoVehiculo=2 AND t.PrecioHora<a.PrecioHora))
            THROW 50001, N'La tarifa de moto debe ser <= automóvil y camión >= automóvil.', 1;";

    private int Guardar(Tarifa objeto, bool nuevo)
    {
        Sesion.ExigirAdministrador();
        if (!nuevo) DatosSql.IdGuardado(objeto.Id);
        string operacion = nuevo
            ? @"INSERT dbo.Tarifa (TipoVehiculo,PrecioHora,PrecioDia,PrecioMensualidad)
                VALUES (@Tipo,@Hora,@Dia,@Mes); SET @Id=CAST(SCOPE_IDENTITY() AS int);"
            : @"IF EXISTS (SELECT 1 FROM dbo.Tarifa WHERE Id=@Id AND TipoVehiculo<>@Tipo)
                    THROW 50021,N'El tipo de una tarifa existente no se puede cambiar.',1;
                UPDATE dbo.Tarifa SET PrecioHora=@Hora,PrecioDia=@Dia,PrecioMensualidad=@Mes WHERE Id=@Id;
                IF @@ROWCOUNT=0 THROW 50002,N'La tarifa ya no existe.',1;";
        // El bloqueo evita validar contra otra edición todavía sin confirmar.
        string sql = DatosSql.Transaccion(@"DECLARE @Bloqueo int;
            SELECT @Bloqueo=Id FROM dbo.Tarifa WITH (UPDLOCK,HOLDLOCK);"
            + operacion + ComprobarOrden + "SELECT @Id;");
        return DatosSql.Insertar(sql, DatosSql.Entero("@Id", objeto.Id), DatosSql.Entero("@Tipo", (int)objeto.TipoVehiculo),
            DatosSql.Dinero("@Hora", objeto.PrecioHora), DatosSql.Dinero("@Dia", objeto.PrecioDia), DatosSql.Dinero("@Mes", objeto.PrecioMensualidad));
    }

    public int Insertar(Tarifa objeto) { objeto.Id = Guardar(objeto, true); return objeto.Id; }
    public void Actualizar(Tarifa objeto) => Guardar(objeto, false);
    public void Eliminar(int id)
    {
        Sesion.ExigirAdministrador();
        DatosSql.IdGuardado(id);
        DatosSql.Cambiar(DatosSql.Transaccion(@"
            DECLARE @Tipo int;
            SELECT @Tipo=TipoVehiculo FROM dbo.Tarifa WITH (UPDLOCK,HOLDLOCK) WHERE Id=@Id;
            IF @Tipo IS NULL THROW 50017,N'La tarifa no existe.',1;
            IF EXISTS (SELECT 1 FROM dbo.Vehiculo WHERE TipoVehiculo=@Tipo OR @Tipo=0)
                THROW 50018,N'No se puede borrar una tarifa necesaria para vehículos registrados.',1;
            DELETE FROM dbo.Tarifa WHERE Id=@Id;"), DatosSql.Entero("@Id", id));
    }

    public void ConfigurarVehiculo(Vehiculo vehiculo)
    {
        List<Tarifa> tarifas = Listar();
        Tarifa? baseAuto = tarifas.Find(t => t.TipoVehiculo == TipoVehiculo.Automovil);
        Tarifa? propia = tarifas.Find(t => t.TipoVehiculo == vehiculo.TipoVehiculo);
        if (baseAuto == null || propia == null) throw new InvalidOperationException("Falta configurar la tarifa del vehículo o la del automóvil.");
        ConfigurarVehiculo(vehiculo, baseAuto.PrecioHora, propia.PrecioHora);
    }

    internal static void ConfigurarVehiculo(Vehiculo vehiculo, decimal precioBase, decimal precioTipo)
    {
        vehiculo.TarifaBaseHora = precioBase;
        if (vehiculo is Motocicleta moto) moto.DescuentoHora = precioBase - precioTipo;
        if (vehiculo is Camion camion) camion.RecargoHora = precioTipo - precioBase;
    }
}
