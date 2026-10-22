using System.Data;
using Microsoft.Data.SqlClient;
using LibreriaClases.Modelos;
using LibreriaClases.Enumeraciones;
using LibreriaClases.Logica;

namespace LibreriaClases.Datos;

public class VehiculoRepositorio
{
    // Los alias permiten reutilizar la lectura también en movimientos y pagos.
    internal const string Columnas = @"v.Id,v.Placa,v.ClienteId,v.TipoVehiculo,v.NumeroPuertas,v.Cilindraje,v.CapacidadToneladas,
        c.Nombres AS c_Nombres,c.Apellidos AS c_Apellidos,c.Telefono AS c_Telefono,
        c.Membresia AS c_Membresia,c.FechaRegistro AS c_FechaRegistro,
        ta.PrecioHora AS t_Base,tt.PrecioHora AS t_Propia";
    internal const string Tablas = @"dbo.Vehiculo v JOIN dbo.Cliente c ON c.Id=v.ClienteId
        LEFT JOIN dbo.Tarifa ta ON ta.TipoVehiculo=0 LEFT JOIN dbo.Tarifa tt ON tt.TipoVehiculo=v.TipoVehiculo";

    internal static Vehiculo Leer(SqlDataReader lector)
    {
        Cliente cliente = new Cliente(lector.GetString(lector.GetOrdinal("c_Nombres")),
            lector.GetString(lector.GetOrdinal("c_Apellidos")), lector.GetString(lector.GetOrdinal("c_Telefono")))
        {
            Id = lector.GetInt32(lector.GetOrdinal("ClienteId")),
            Membresia = lector.GetBoolean(lector.GetOrdinal("c_Membresia")),
            FechaRegistro = lector.GetDateTime(lector.GetOrdinal("c_FechaRegistro"))
        };
        string placa = lector.GetString(lector.GetOrdinal("Placa"));
        TipoVehiculo tipo = (TipoVehiculo)lector.GetInt32(lector.GetOrdinal("TipoVehiculo"));
        Vehiculo vehiculo;
        // Se reconstruye la hija: no se pierde el polimorfismo al leer SQL.
        switch (tipo)
        {
            case TipoVehiculo.Automovil:
                vehiculo = new Automovil(placa, cliente, lector.GetInt32(lector.GetOrdinal("NumeroPuertas")));
                break;
            case TipoVehiculo.Motocicleta:
                vehiculo = new Motocicleta(placa, cliente, lector.GetInt32(lector.GetOrdinal("Cilindraje")));
                break;
            case TipoVehiculo.Camion:
                vehiculo = new Camion(placa, cliente, lector.GetDecimal(lector.GetOrdinal("CapacidadToneladas")));
                break;
            default: throw new InvalidOperationException("Tipo de vehículo desconocido en la base.");
        }
        vehiculo.Id = lector.GetInt32(lector.GetOrdinal("Id"));
        if (lector.IsDBNull(lector.GetOrdinal("t_Base")) || lector.IsDBNull(lector.GetOrdinal("t_Propia")))
            throw new InvalidOperationException("Falta configurar una tarifa por tipo.");
        TarifaRepositorio.ConfigurarVehiculo(vehiculo, lector.GetDecimal(lector.GetOrdinal("t_Base")), lector.GetDecimal(lector.GetOrdinal("t_Propia")));
        return vehiculo;
    }

    public List<Vehiculo> Listar(string busqueda = "")
    {
        List<Vehiculo> lista = new List<Vehiculo>();
        using (SqlConnection conexion = Conexion.Abrir())
        using (SqlCommand comando = DatosSql.Crear(conexion,
            "SELECT " + Columnas + " FROM " + Tablas + " WHERE v.Placa LIKE @Busqueda ORDER BY v.Placa",
            DatosSql.Texto("@Busqueda", "%" + busqueda.Trim() + "%", 200)))
        using (SqlDataReader lector = comando.ExecuteReader())
            while (lector.Read()) lista.Add(Leer(lector));
        return lista;
    }

    public Vehiculo? ObtenerPorId(int id)
    {
        DatosSql.IdGuardado(id);
        Vehiculo? vehiculo;
        using (SqlConnection conexion = Conexion.Abrir())
        using (SqlCommand comando = DatosSql.Crear(conexion,
            "SELECT " + Columnas + " FROM " + Tablas + " WHERE v.Id=@Id", DatosSql.Entero("@Id", id)))
        using (SqlDataReader lector = comando.ExecuteReader())
            vehiculo = lector.Read() ? Leer(lector) : null;
        return vehiculo;
    }

    private static SqlParameter[] Parametros(Vehiculo objeto)
    {
        DatosSql.IdGuardado(objeto.Propietario.Id);
        SqlParameter puertas = DatosSql.Nulo("@Puertas", SqlDbType.Int);
        SqlParameter cilindros = DatosSql.Nulo("@Cilindraje", SqlDbType.Int);
        SqlParameter capacidad = new SqlParameter("@Capacidad", SqlDbType.Decimal) { Precision = 8, Scale = 2, Value = DBNull.Value };
        if (objeto is Automovil auto) puertas.Value = auto.NumeroPuertas;
        else if (objeto is Motocicleta moto) cilindros.Value = moto.Cilindraje;
        else if (objeto is Camion camion) capacidad.Value = camion.CapacidadToneladas;
        else throw new ArgumentException("Clase de vehículo no admitida.");
        return new SqlParameter[] { DatosSql.Texto("@Placa", objeto.Placa, 15), DatosSql.Entero("@Cliente", objeto.Propietario.Id),
            DatosSql.Entero("@Tipo", (int)objeto.TipoVehiculo), puertas, cilindros, capacidad };
    }

    public int Insertar(Vehiculo objeto)
    {
        Sesion.ExigirAdministrador();
        objeto.Id = DatosSql.Insertar(DatosSql.Transaccion(@"
            DECLARE @Bloqueo int;
            SELECT @Bloqueo=Id FROM dbo.Tarifa WITH (UPDLOCK,HOLDLOCK);
            IF NOT EXISTS (SELECT 1 FROM dbo.Tarifa WHERE TipoVehiculo=0)
                OR NOT EXISTS (SELECT 1 FROM dbo.Tarifa WHERE TipoVehiculo=@Tipo)
                THROW 50022,N'Configure las tarifas antes de registrar el vehículo.',1;
            INSERT dbo.Vehiculo (Placa,ClienteId,TipoVehiculo,NumeroPuertas,Cilindraje,CapacidadToneladas)
                VALUES (@Placa,@Cliente,@Tipo,@Puertas,@Cilindraje,@Capacidad);
            SELECT CAST(SCOPE_IDENTITY() AS int);"), Parametros(objeto));
        return objeto.Id;
    }

    public void Actualizar(Vehiculo objeto)
    {
        Sesion.ExigirAdministrador();
        DatosSql.IdGuardado(objeto.Id);
        using SqlConnection conexion = Conexion.Abrir();
        using SqlCommand comando = DatosSql.Crear(conexion, DatosSql.Transaccion(@"
            DECLARE @TipoAnterior int;
            SELECT @TipoAnterior=TipoVehiculo FROM dbo.Vehiculo WITH (UPDLOCK,HOLDLOCK) WHERE Id=@Id;
            IF @TipoAnterior IS NULL THROW 50003,N'El vehículo ya no existe.',1;
            IF @TipoAnterior<>@Tipo AND EXISTS (SELECT 1 FROM dbo.Movimiento WHERE VehiculoId=@Id)
                THROW 50004,N'No se puede cambiar el tipo de un vehículo con historial.',1;
            SELECT @TipoAnterior=TipoVehiculo FROM dbo.Tarifa WITH (UPDLOCK,HOLDLOCK) WHERE TipoVehiculo=@Tipo;
            IF NOT EXISTS (SELECT 1 FROM dbo.Tarifa WHERE TipoVehiculo=0)
                OR NOT EXISTS (SELECT 1 FROM dbo.Tarifa WHERE TipoVehiculo=@Tipo)
                THROW 50022,N'Configure las tarifas antes de cambiar el vehículo.',1;
            UPDATE dbo.Vehiculo SET Placa=@Placa,ClienteId=@Cliente,TipoVehiculo=@Tipo,
                NumeroPuertas=@Puertas,Cilindraje=@Cilindraje,CapacidadToneladas=@Capacidad WHERE Id=@Id;"), Parametros(objeto));
        comando.Parameters.Add(DatosSql.Entero("@Id", objeto.Id));
        comando.ExecuteNonQuery();
    }

    public void Eliminar(int id)
    {
        Sesion.ExigirAdministrador();
        DatosSql.IdGuardado(id);
        DatosSql.Cambiar("DELETE FROM dbo.Vehiculo WHERE Id=@Id", DatosSql.Entero("@Id", id));
    }
}
