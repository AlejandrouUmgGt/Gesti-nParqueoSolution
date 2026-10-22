using LibreriaClases.Datos;
using LibreriaClases.Enumeraciones;
using LibreriaClases.Modelos;

namespace LibreriaClases.Logica;

public class GestorParqueo
{
    // SQL guarda segundos completos: así una fecha no cambia por redondeo al persistirla.
    public static DateTime SinFracciones(DateTime fecha) => new DateTime(
        fecha.Ticks - fecha.Ticks % TimeSpan.TicksPerSecond, fecha.Kind);

    public static int CalcularHoras(TimeSpan duracion)
    {
        if (duracion < TimeSpan.Zero) throw new ArgumentException("La duración no puede ser negativa.");
        double horas = Math.Max(1, Math.Ceiling(duracion.TotalHours));
        if (horas > int.MaxValue) throw new ArgumentException("La duración es demasiado grande.");
        return (int)horas;
    }

    public decimal CalcularMonto(Movimiento movimiento, DateTime salida)
    {
        int horas = CalcularHoras(movimiento.CalcularDuracion(salida));
        // Esta llamada demuestra polimorfismo: la clase real decide el cálculo.
        return movimiento.Vehiculo.CalcularTarifa(horas);
    }

    public int RegistrarEntrada(int vehiculoId, int espacioId)
        => RegistrarEntrada(vehiculoId, espacioId, DateTime.Now);

    public int RegistrarEntrada(int vehiculoId, int espacioId, DateTime entrada)
    {
        Sesion.ExigirSesion();
        DatosSql.IdGuardado(vehiculoId);
        DatosSql.IdGuardado(espacioId);
        if (entrada > DateTime.Now) throw new ArgumentException("La entrada no puede estar en el futuro.");
        entrada = SinFracciones(entrada);
        string sql = DatosSql.Transaccion(@"
            DECLARE @Vehiculo int,@Disponible bit;
            SELECT @Vehiculo=Id FROM dbo.Vehiculo WITH (UPDLOCK,HOLDLOCK) WHERE Id=@VehiculoId;
            IF @Vehiculo IS NULL THROW 50010,N'El vehículo no existe.',1;
            SELECT @Disponible=Disponible FROM dbo.EspacioParqueo WITH (UPDLOCK,HOLDLOCK) WHERE Id=@EspacioId;
            IF @Disponible IS NULL THROW 50011,N'El espacio no existe.',1;
            IF @Disponible=0 THROW 50012,N'El espacio está ocupado.',1;
            IF EXISTS (SELECT 1 FROM dbo.Movimiento WITH (UPDLOCK,HOLDLOCK)
                WHERE FechaHoraSalida IS NULL AND (VehiculoId=@VehiculoId OR EspacioId=@EspacioId))
                THROW 50013,N'El vehículo ya está dentro o el espacio tiene una entrada activa.',1;
            IF EXISTS (SELECT 1 FROM dbo.Movimiento WHERE FechaHoraSalida>@Entrada
                AND (VehiculoId=@VehiculoId OR EspacioId=@EspacioId))
                THROW 50014,N'La fecha de entrada se solapa con una visita anterior.',1;
            INSERT dbo.Movimiento (VehiculoId,EspacioId,FechaHoraEntrada) VALUES (@VehiculoId,@EspacioId,@Entrada);
            DECLARE @NuevoId int=CAST(SCOPE_IDENTITY() AS int);
            UPDATE dbo.EspacioParqueo SET Disponible=0 WHERE Id=@EspacioId;
            SELECT @NuevoId;");
        return DatosSql.Insertar(sql, DatosSql.Entero("@VehiculoId", vehiculoId),
            DatosSql.Entero("@EspacioId", espacioId), DatosSql.Fecha("@Entrada", entrada));
    }

    public Pago PrepararSalida(int movimientoId)
    {
        Sesion.ExigirSesion();
        Movimiento movimiento = ObtenerActivo(movimientoId);
        DateTime salida = SinFracciones(DateTime.Now);
        return new Pago(movimiento, MetodoPago.Efectivo, CalcularMonto(movimiento, salida), salida);
    }

    private static Movimiento ObtenerActivo(int id)
    {
        Movimiento movimiento = new MovimientoRepositorio().ObtenerPorId(id)
            ?? throw new InvalidOperationException("El movimiento no existe.");
        if (!movimiento.Activo) throw new InvalidOperationException("El movimiento ya tiene salida.");
        return movimiento;
    }

    public Pago RegistrarSalida(int movimientoId, MetodoPago metodo, decimal monto)
        => RegistrarSalida(movimientoId, metodo, monto, DateTime.Now);

    public Pago RegistrarSalida(int movimientoId, MetodoPago metodo, decimal monto, DateTime salida)
    {
        Sesion.ExigirSesion();
        if (salida > DateTime.Now) throw new ArgumentException("La salida no puede estar en el futuro.");
        salida = SinFracciones(salida);
        Movimiento movimiento = ObtenerActivo(movimientoId);
        decimal calculado = CalcularMonto(movimiento, salida);
        if (monto != calculado) throw new ArgumentException($"El monto debe ser exactamente Q{calculado:0.00}. Actualice la cotización.");
        Pago pago = new Pago(movimiento, metodo, monto, salida);
        string sql = DatosSql.Transaccion(@"
            DECLARE @Bloqueo int;
            -- Todas las operaciones bloquean primero vehículo y luego espacio.
            SELECT @Bloqueo=Id FROM dbo.Vehiculo WITH (UPDLOCK,HOLDLOCK) WHERE Id=@Vehiculo;
            SELECT @Bloqueo=Id FROM dbo.EspacioParqueo WITH (UPDLOCK,HOLDLOCK) WHERE Id=@Espacio;
            IF NOT EXISTS (SELECT 1 FROM dbo.Movimiento WITH (UPDLOCK,HOLDLOCK)
                WHERE Id=@Id AND VehiculoId=@Vehiculo AND EspacioId=@Espacio
                    AND FechaHoraSalida IS NULL AND FechaHoraEntrada=@Entrada)
                THROW 50015,N'El movimiento cambió o ya salió; actualice la consulta.',1;
            SELECT @Bloqueo=Id FROM dbo.Tarifa WITH (UPDLOCK,HOLDLOCK);
            IF NOT EXISTS (SELECT 1 FROM dbo.Tarifa WHERE TipoVehiculo=0 AND PrecioHora=@Base)
                OR NOT EXISTS (SELECT 1 FROM dbo.Tarifa WHERE TipoVehiculo=@Tipo AND PrecioHora=@Hora)
                THROW 50016,N'Las tarifas cambiaron; vuelva a calcular el monto.',1;
            UPDATE dbo.Movimiento SET FechaHoraSalida=@Salida WHERE Id=@Id;
            INSERT dbo.Pago (MovimientoId,Metodo,Monto,Fecha) VALUES (@Id,@Metodo,@Monto,@Salida);
            DECLARE @PagoId int=CAST(SCOPE_IDENTITY() AS int);
            UPDATE dbo.EspacioParqueo SET Disponible=1 WHERE Id=@Espacio;
            SELECT @PagoId;");
        pago.Id = DatosSql.Insertar(sql, DatosSql.Entero("@Id", movimiento.Id),
            DatosSql.Entero("@Vehiculo", movimiento.Vehiculo.Id), DatosSql.Entero("@Espacio", movimiento.Espacio.Id),
            DatosSql.Fecha("@Entrada", movimiento.FechaHoraEntrada), DatosSql.Fecha("@Salida", salida),
            DatosSql.Entero("@Metodo", (int)metodo), DatosSql.Dinero("@Monto", monto),
            DatosSql.Entero("@Tipo", (int)movimiento.Vehiculo.TipoVehiculo),
            DatosSql.Dinero("@Base", movimiento.Vehiculo.TarifaBaseHora), DatosSql.Dinero("@Hora", movimiento.Vehiculo.CalcularTarifa(1)));
        movimiento.FechaHoraSalida = salida;
        movimiento.Espacio.Disponible = true;
        return pago;
    }
}
