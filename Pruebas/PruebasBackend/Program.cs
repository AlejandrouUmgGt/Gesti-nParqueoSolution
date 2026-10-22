using LibreriaClases.Datos;
using LibreriaClases.Enumeraciones;
using LibreriaClases.Logica;
using LibreriaClases.Modelos;
using Microsoft.Data.SqlClient;

namespace PruebasBackend;

// Consola sin paquetes de pruebas: un fallo produce código de salida 1.
internal static class Program
{
    private static int comprobaciones;

    private static int Main(string[] args)
    {
        try
        {
            ProbarModelos();
            ProbarSeguridad();
            ProbarPermisosSinSesion();
            if (args.Contains("--sql")) ProbarSql();
            else Console.WriteLine("SQL Server: NO EJECUTADO (usar --sql en Windows con una base de desarrollo).");
            Console.WriteLine($"Correctas: {comprobaciones} comprobaciones.");
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine($"FALLO: {error.GetType().Name}: {error.Message}");
            return 1;
        }
        finally { Sesion.Cerrar(); }
    }

    private static void Comprobar(bool condicion, string caso)
    {
        if (!condicion) throw new InvalidOperationException("No se cumplió: " + caso);
        comprobaciones++;
    }

    private static void RechazarArgumento(Action accion, string caso)
    {
        try { accion(); }
        catch (ArgumentException) { comprobaciones++; return; }
        throw new InvalidOperationException("No rechazó: " + caso);
    }

    private static void RechazarOperacion(Action accion, string caso)
    {
        try { accion(); }
        catch (InvalidOperationException) { comprobaciones++; return; }
        throw new InvalidOperationException("No rechazó: " + caso);
    }

    private static void RechazarSql(Action accion, int numero, string caso)
    {
        try { accion(); }
        catch (SqlException error) when (error.Number == numero) { comprobaciones++; return; }
        throw new InvalidOperationException("No rechazó con el error previsto: " + caso);
    }

    private static void ProbarModelos()
    {
        Cliente cliente = new Cliente(" Ana ", "López", "5555-0101");
        Comprobar(cliente.NombreCompleto == "Ana López", "nombre calculado y recorte");
        Comprobar(cliente.Id == 0, "objeto aún sin guardar");
        RechazarArgumento(() => cliente.Id = -1, "Id negativo");
        RechazarArgumento(() => cliente.Nombres = " ", "nombre vacío");
        RechazarArgumento(() => cliente.Apellidos = new string('A', 81), "apellido demasiado largo");
        RechazarArgumento(() => cliente.Telefono = "", "teléfono vacío");
        Empleado empleado = new Empleado("Luis", "Pérez", "5555-0102", "Caja", "cajero", Seguridad.CrearHash("Prueba2026!")) { Rol = RolUsuario.Cajero };
        List<Persona> personas = new List<Persona> { cliente, empleado };
        Comprobar(personas[0].ObtenerResumen().StartsWith("Cliente:"), "resumen polimórfico de cliente");
        Comprobar(personas[1].ObtenerResumen().StartsWith("Empleado:"), "resumen polimórfico de empleado");
        Comprobar(personas[0].ObtenerRol() == "Cliente", "rol de cliente");
        Comprobar(personas[1].ObtenerRol() == "Cajero", "rol de empleado");
        RechazarArgumento(() => empleado.Rol = (RolUsuario)99, "rol fuera de catálogo");
        RechazarArgumento(() => empleado.ClaveHash = "texto plano", "contraseña sin hash");

        Automovil auto = new Automovil("p001aaa", cliente, 4);
        Motocicleta moto = new Motocicleta("m001aaa", cliente, 150);
        Camion camion = new Camion("c001aaa", cliente, 5);
        List<Vehiculo> vehiculos = new List<Vehiculo> { auto, moto, camion };
        Comprobar(vehiculos[0].CalcularTarifa(2) == 20m, "automóvil dos horas");
        Comprobar(vehiculos[1].CalcularTarifa(2) == 14m, "moto con descuento");
        Comprobar(vehiculos[2].CalcularTarifa(2) == 30m, "camión con recargo");
        Comprobar(auto.Placa == "P001AAA", "normalización de placa");
        Comprobar(auto.ObtenerDescripcion().Contains("4 puertas"), "descripción de automóvil");
        Comprobar(moto.ObtenerDescripcion().Contains("150 cc"), "descripción de motocicleta");
        Comprobar(camion.ObtenerDescripcion().Contains("5 toneladas"), "descripción de camión");
        RechazarArgumento(() => auto.NumeroPuertas = 0, "cero puertas");
        RechazarArgumento(() => moto.Cilindraje = -1, "cilindraje negativo");
        RechazarArgumento(() => camion.CapacidadToneladas = 0, "capacidad cero");
        RechazarArgumento(() => auto.Placa = "", "placa vacía");
        RechazarArgumento(() => auto.CalcularTarifa(0), "cero horas");
        RechazarArgumento(() => auto.TarifaBaseHora = 0, "tarifa cero");
        RechazarArgumento(() => moto.DescuentoHora = -1, "descuento negativo");
        RechazarArgumento(() => camion.RecargoHora = -1, "recargo negativo");
        moto.DescuentoHora = 10;
        RechazarOperacion(() => moto.CalcularTarifa(1), "descuento igual a base");
        moto.DescuentoHora = 3;
        auto.TarifaBaseHora = 12;
        moto.TarifaBaseHora = 12;
        moto.DescuentoHora = 4;
        camion.TarifaBaseHora = 12;
        camion.RecargoHora = 6;
        Comprobar(auto.CalcularTarifa(2) == 24m && moto.CalcularTarifa(2) == 16m && camion.CalcularTarifa(2) == 36m, "tarifas configurables sin doble ajuste");

        EspacioParqueo espacio = new EspacioParqueo(1, 1);
        RechazarArgumento(() => espacio.Nivel = 0, "nivel cero");
        RechazarArgumento(() => espacio.Numero = 0, "número cero");
        Tarifa tarifa = new Tarifa(TipoVehiculo.Automovil, 10, 60, 600);
        RechazarArgumento(() => tarifa.PrecioHora = 1.001m, "más de dos decimales");
        RechazarArgumento(() => tarifa.TipoVehiculo = (TipoVehiculo)99, "tipo desconocido");
        RechazarArgumento(() => tarifa.PrecioDia = -1, "precio diario negativo");
        DateTime entrada = new DateTime(2026, 1, 1, 10, 0, 0);
        Movimiento movimiento = new Movimiento(auto, espacio, entrada) { FechaHoraSalida = entrada.AddMinutes(61) };
        Comprobar(!movimiento.Activo && movimiento.Duracion == TimeSpan.FromMinutes(61), "duración de movimiento cerrado");
        RechazarArgumento(() => movimiento.CalcularDuracion(entrada.AddSeconds(-1)), "salida anterior a entrada");
        Comprobar(GestorParqueo.CalcularHoras(TimeSpan.Zero) == 1, "mínimo una hora");
        Comprobar(GestorParqueo.CalcularHoras(TimeSpan.FromHours(1)) == 1, "hora exacta");
        Comprobar(GestorParqueo.CalcularHoras(TimeSpan.FromSeconds(3601)) == 2, "fracción inicia otra hora");
        Comprobar(GestorParqueo.CalcularHoras(TimeSpan.FromHours(25)) == 25, "sin tarifa diaria automática");
        RechazarArgumento(() => GestorParqueo.CalcularHoras(TimeSpan.FromSeconds(-1)), "duración negativa");
        Comprobar(new GestorParqueo().CalcularMonto(movimiento, entrada.AddMinutes(61)) == 24, "gestor usa método polimórfico");
        Comprobar(GestorParqueo.SinFracciones(entrada.AddMilliseconds(999)) == entrada, "normalización sin redondeo al segundo siguiente");
        Pago pago = new Pago(movimiento, MetodoPago.Efectivo, 24m, entrada.AddMinutes(61));
        RechazarArgumento(() => pago.Metodo = (MetodoPago)99, "método desconocido");
        RechazarArgumento(() => pago.Monto = 0, "pago cero");
    }

    private static void ProbarSeguridad()
    {
        const string demo = "PBKDF2-SHA256$210000$bNh1YBfcShyUq1oTZuKeMg==$5cBspFfA8UrG2PlLfn38d752Rk5nfw34HhnEdEjvGYk=";
        Comprobar(Seguridad.VerificarClave("ParqueoDemo2026!", demo), "compatibilidad con administrador SQL");
        Comprobar(!Seguridad.VerificarClave("incorrecta", demo), "contraseña incorrecta");
        string primero = Seguridad.CrearHash("OtraClave2026!");
        string segundo = Seguridad.CrearHash("OtraClave2026!");
        Comprobar(primero != segundo, "sal aleatoria por contraseña");
        Comprobar(Seguridad.VerificarClave("OtraClave2026!", primero), "hash nuevo verificable");
        Comprobar(!Seguridad.VerificarClave(null, primero), "contraseña nula");
        Comprobar(!Seguridad.VerificarClave("abc", "formato inválido"), "hash mal formado");
        Comprobar(!Seguridad.EsHashValido(demo.Replace("210000", "999999999")), "límite de iteraciones al leer hash");
        Comprobar(!Seguridad.EsHashValido(demo.Replace("210000", "0")), "cero iteraciones");
        Comprobar(!Seguridad.EsHashValido("PBKDF2-SHA256$210000$???$???"), "base64 inválido");
        Comprobar(!Seguridad.EsHashValido("PBKDF2-SHA256$210000$YQ==$YQ=="), "sal y resultado de tamaño incorrecto");
        RechazarArgumento(() => Seguridad.CrearHash("corta"), "contraseña demasiado corta");
    }

    private static void ProbarPermisosSinSesion()
    {
        Sesion.Cerrar();
        Comprobar(Sesion.EmpleadoActual == null && !Sesion.EsAdministrador, "sesión cerrada");
        RechazarOperacion(() => Sesion.ExigirSesion(), "operación sin sesión");
        RechazarOperacion(() => Sesion.ExigirAdministrador(), "administración sin sesión");
        RechazarOperacion(() => new GestorParqueo().RegistrarEntrada(1, 1), "entrada sin sesión antes de acceder a SQL");
        RechazarOperacion(() => new ClienteRepositorio().Eliminar(1), "borrado sin sesión antes de acceder a SQL");
    }

    private static void ProbarSql()
    {
        Console.WriteLine("SQL: usará exclusivamente los registros temporales E2 creados por esta prueba.");
        string usuario = Environment.GetEnvironmentVariable("PARQUEO_PRUEBA_USUARIO") ?? "admin";
        string clave = Environment.GetEnvironmentVariable("PARQUEO_PRUEBA_CLAVE") ?? "ParqueoDemo2026!";
        Comprobar(Sesion.Iniciar(usuario, clave) && Sesion.EsAdministrador, "login de administrador Windows/SQL");
        ClienteRepositorio clientes = new ClienteRepositorio();
        VehiculoRepositorio vehiculos = new VehiculoRepositorio();
        EspacioParqueoRepositorio espacios = new EspacioParqueoRepositorio();
        MovimientoRepositorio movimientos = new MovimientoRepositorio();
        PagoRepositorio pagos = new PagoRepositorio();
        EmpleadoRepositorio empleados = new EmpleadoRepositorio();
        GestorParqueo gestor = new GestorParqueo();
        string marca = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        Cliente cliente = new Cliente("Prueba E2", marca, "5555-9999");
        Automovil auto = new Automovil("E2A" + marca, cliente, 4);
        Motocicleta moto = new Motocicleta("E2M" + marca, cliente, 125);
        Camion camion = new Camion("E2C" + marca, cliente, 3);
        EspacioParqueo espacio = new EspacioParqueo(1, 2000000000 + Random.Shared.Next(1, 1000000));
        Empleado cajero = new Empleado("Prueba E2", marca, "5555-9999", "Caja", "E2" + marca, Seguridad.CrearHash("CajeroPrueba2026!")) { Rol = RolUsuario.Cajero };
        int movimientoId = 0;
        int pagoId = 0;
        try
        {
            clientes.Insertar(cliente);
            cliente.Membresia = true;
            clientes.Actualizar(cliente);
            Comprobar(clientes.ObtenerPorId(cliente.Id)?.Membresia == true, "CRUD cliente");
            espacios.Insertar(espacio);
            vehiculos.Insertar(auto);
            vehiculos.Insertar(moto);
            vehiculos.Insertar(camion);
            Comprobar(vehiculos.ObtenerPorId(auto.Id) is Automovil, "SQL reconstruye automóvil");
            Comprobar(vehiculos.ObtenerPorId(moto.Id) is Motocicleta, "SQL reconstruye motocicleta");
            Comprobar(vehiculos.ObtenerPorId(camion.Id) is Camion, "SQL reconstruye camión");
            empleados.Insertar(cajero);
            Comprobar(Sesion.Iniciar(cajero.Usuario, "CajeroPrueba2026!") && !Sesion.EsAdministrador, "login de cajero");
            RechazarOperacion(() => clientes.Actualizar(cliente), "cajero no modifica clientes");
            movimientoId = gestor.RegistrarEntrada(auto.Id, espacio.Id, DateTime.Now.AddMinutes(-65));
            Comprobar(espacios.ObtenerPorId(espacio.Id)?.Disponible == false, "entrada ocupa espacio");
            // Otro espacio permite comprobar específicamente el vehículo duplicado.
            RechazarSql(() => gestor.RegistrarEntrada(moto.Id, espacio.Id), 50012, "espacio ocupado");
            EspacioParqueo otro = new EspacioParqueo(2, espacio.Nivel);
            Comprobar(Sesion.Iniciar(usuario, clave), "volver a administrador");
            espacios.Insertar(otro);
            try { RechazarSql(() => gestor.RegistrarEntrada(auto.Id, otro.Id), 50013, "vehículo ya dentro"); }
            finally { espacios.Eliminar(otro.Id); }
            Pago cotizacion = gestor.PrepararSalida(movimientoId);
            Comprobar(cotizacion.Monto == cotizacion.Movimiento.Vehiculo.CalcularTarifa(2), "dos horas iniciadas con tarifa SQL");
            RechazarArgumento(() => gestor.RegistrarSalida(movimientoId, MetodoPago.Efectivo, cotizacion.Monto + 1, cotizacion.Fecha), "monto incorrecto");
            Comprobar(movimientos.ObtenerPorId(movimientoId)?.Activo == true && espacios.ObtenerPorId(espacio.Id)?.Disponible == false,
                "salida rechazada no modifica estado");
            Pago pago = gestor.RegistrarSalida(movimientoId, MetodoPago.Tarjeta, cotizacion.Monto, cotizacion.Fecha);
            pagoId = pago.Id;
            Comprobar(espacios.ObtenerPorId(espacio.Id)?.Disponible == true && movimientos.ObtenerPorId(movimientoId)?.Activo == false, "salida libera espacio");
            Comprobar(pagos.ObtenerPorId(pago.Id)?.Monto == cotizacion.Monto, "pago persistido");
            RechazarOperacion(() => gestor.RegistrarSalida(movimientoId, MetodoPago.Efectivo, cotizacion.Monto), "salida duplicada");
            pago.Metodo = MetodoPago.Efectivo;
            pagos.Actualizar(pago);
            Comprobar(pagos.ObtenerPorId(pago.Id)?.Metodo == MetodoPago.Efectivo, "corrección de método de pago");
            Comprobar(pagos.IngresosPorDia(DateTime.Today, DateTime.Today.AddDays(1)).Rows.Count > 0, "reporte diario");
            Comprobar(pagos.IngresosPorMes(DateTime.Today, DateTime.Today.AddDays(1)).Rows.Count > 0, "reporte mensual");
            Comprobar(movimientos.OcupacionPorHora(DateTime.Today.AddDays(-1)).Rows.Count == 24, "reporte de 24 horas pasadas");
            Comprobar(movimientos.ClientesRecurrentes(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1), 1).Rows.Count > 0, "reporte de clientes");
            pagos.Eliminar(pagoId);
            pagoId = 0;
            Comprobar(movimientos.ObtenerPorId(movimientoId)?.Activo == true && espacios.ObtenerPorId(espacio.Id)?.Disponible == false, "anulación revierte salida");
            movimientos.Eliminar(movimientoId);
            movimientoId = 0;
            Comprobar(espacios.ObtenerPorId(espacio.Id)?.Disponible == true, "cancelación de entrada libera espacio");
            cajero.Activo = false;
            empleados.Actualizar(cajero);
            Comprobar(!Sesion.Iniciar(cajero.Usuario, "CajeroPrueba2026!"), "empleado inactivo no inicia sesión");
            Comprobar(Sesion.EmpleadoActual == null, "login fallido elimina sesión anterior");
        }
        finally
        {
            // Solo se retiran los identificadores creados por esta ejecución.
            if (!Sesion.Iniciar(usuario, clave) || !Sesion.EsAdministrador)
                throw new InvalidOperationException("No se pudo iniciar sesión para limpiar las pruebas E2.");
            if (pagoId > 0) pagos.Eliminar(pagoId);
            if (movimientoId > 0) movimientos.Eliminar(movimientoId);
            if (auto.Id > 0) vehiculos.Eliminar(auto.Id);
            if (moto.Id > 0) vehiculos.Eliminar(moto.Id);
            if (camion.Id > 0) vehiculos.Eliminar(camion.Id);
            if (espacio.Id > 0) espacios.Eliminar(espacio.Id);
            if (cajero.Id > 0) empleados.Eliminar(cajero.Id);
            if (cliente.Id > 0) clientes.Eliminar(cliente.Id);
        }
    }
}
