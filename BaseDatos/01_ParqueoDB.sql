-- Etapa 1. Ejecutar completo en SSMS con autenticación de Windows.
-- No elimina datos ni crea usuarios o logins de SQL Server.
USE master;
GO
IF DB_ID(N'ParqueoDB') IS NULL
    EXEC(N'CREATE DATABASE ParqueoDB');
GO
USE ParqueoDB;
GO
-- Opciones exigidas por los índices filtrados.
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET ARITHABORT ON;
SET NUMERIC_ROUNDABORT OFF;
SET XACT_ABORT ON;
GO
BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.Empleado', N'U') IS NULL
    CREATE TABLE dbo.Empleado (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Empleado PRIMARY KEY,
        Nombres nvarchar(80) NOT NULL,
        Apellidos nvarchar(80) NOT NULL,
        Telefono nvarchar(25) NOT NULL,
        Cargo nvarchar(60) NOT NULL,
        Usuario nvarchar(50) NOT NULL CONSTRAINT UQ_Empleado_Usuario UNIQUE,
        ClaveHash varchar(200) NOT NULL,
        Activo bit NOT NULL CONSTRAINT DF_Empleado_Activo DEFAULT 1,
        Rol int NOT NULL,
        CONSTRAINT CK_Empleado_Rol CHECK (Rol IN (0,1)),
        CONSTRAINT CK_Empleado_Nombres CHECK (LEN(LTRIM(RTRIM(Nombres))) > 0),
        CONSTRAINT CK_Empleado_Apellidos CHECK (LEN(LTRIM(RTRIM(Apellidos))) > 0),
        CONSTRAINT CK_Empleado_Usuario CHECK (LEN(LTRIM(RTRIM(Usuario))) > 0),
        CONSTRAINT CK_Empleado_Hash CHECK (ClaveHash LIKE 'PBKDF2-SHA256$%')
    );

    IF OBJECT_ID(N'dbo.Cliente', N'U') IS NULL
    CREATE TABLE dbo.Cliente (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Cliente PRIMARY KEY,
        Nombres nvarchar(80) NOT NULL,
        Apellidos nvarchar(80) NOT NULL,
        Telefono nvarchar(25) NOT NULL,
        Membresia bit NOT NULL CONSTRAINT DF_Cliente_Membresia DEFAULT 0,
        FechaRegistro datetime2(0) NOT NULL CONSTRAINT DF_Cliente_Fecha DEFAULT SYSDATETIME(),
        CONSTRAINT CK_Cliente_Nombres CHECK (LEN(LTRIM(RTRIM(Nombres))) > 0),
        CONSTRAINT CK_Cliente_Apellidos CHECK (LEN(LTRIM(RTRIM(Apellidos))) > 0)
    );

    IF OBJECT_ID(N'dbo.Vehiculo', N'U') IS NULL
    CREATE TABLE dbo.Vehiculo (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Vehiculo PRIMARY KEY,
        Placa nvarchar(15) NOT NULL CONSTRAINT UQ_Vehiculo_Placa UNIQUE,
        ClienteId int NOT NULL,
        TipoVehiculo int NOT NULL,
        NumeroPuertas int NULL,
        Cilindraje int NULL,
        CapacidadToneladas decimal(8,2) NULL,
        CONSTRAINT FK_Vehiculo_Cliente FOREIGN KEY (ClienteId) REFERENCES dbo.Cliente(Id),
        CONSTRAINT CK_Vehiculo_Placa CHECK (LEN(LTRIM(RTRIM(Placa))) > 0),
        -- Cada fila tiene exclusivamente los datos de su clase hija.
        CONSTRAINT CK_Vehiculo_Subtipo CHECK (
            (TipoVehiculo = 0 AND NumeroPuertas IS NOT NULL AND NumeroPuertas BETWEEN 1 AND 6
                AND Cilindraje IS NULL AND CapacidadToneladas IS NULL)
            OR (TipoVehiculo = 1 AND Cilindraje IS NOT NULL AND Cilindraje > 0
                AND NumeroPuertas IS NULL AND CapacidadToneladas IS NULL)
            OR (TipoVehiculo = 2 AND CapacidadToneladas IS NOT NULL AND CapacidadToneladas > 0
                AND NumeroPuertas IS NULL AND Cilindraje IS NULL)
        )
    );

    IF OBJECT_ID(N'dbo.EspacioParqueo', N'U') IS NULL
    CREATE TABLE dbo.EspacioParqueo (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Espacio PRIMARY KEY,
        Numero int NOT NULL,
        Nivel int NOT NULL,
        Disponible bit NOT NULL CONSTRAINT DF_Espacio_Disponible DEFAULT 1,
        CONSTRAINT UQ_Espacio_NumeroNivel UNIQUE (Numero, Nivel),
        CONSTRAINT CK_Espacio_Numero CHECK (Numero > 0),
        CONSTRAINT CK_Espacio_Nivel CHECK (Nivel > 0)
    );

    IF OBJECT_ID(N'dbo.Tarifa', N'U') IS NULL
    CREATE TABLE dbo.Tarifa (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Tarifa PRIMARY KEY,
        TipoVehiculo int NOT NULL CONSTRAINT UQ_Tarifa_Tipo UNIQUE,
        PrecioHora decimal(10,2) NOT NULL,
        PrecioDia decimal(10,2) NOT NULL,
        PrecioMensualidad decimal(10,2) NOT NULL,
        CONSTRAINT CK_Tarifa_Tipo CHECK (TipoVehiculo IN (0,1,2)),
        CONSTRAINT CK_Tarifa_Precios CHECK (PrecioHora > 0 AND PrecioDia > 0 AND PrecioMensualidad > 0)
    );

    IF OBJECT_ID(N'dbo.Movimiento', N'U') IS NULL
    CREATE TABLE dbo.Movimiento (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Movimiento PRIMARY KEY,
        VehiculoId int NOT NULL,
        EspacioId int NOT NULL,
        FechaHoraEntrada datetime2(0) NOT NULL CONSTRAINT DF_Movimiento_Entrada DEFAULT SYSDATETIME(),
        FechaHoraSalida datetime2(0) NULL,
        CONSTRAINT FK_Movimiento_Vehiculo FOREIGN KEY (VehiculoId) REFERENCES dbo.Vehiculo(Id),
        CONSTRAINT FK_Movimiento_Espacio FOREIGN KEY (EspacioId) REFERENCES dbo.EspacioParqueo(Id),
        CONSTRAINT CK_Movimiento_Fechas CHECK (FechaHoraSalida IS NULL OR FechaHoraSalida >= FechaHoraEntrada)
    );

    IF OBJECT_ID(N'dbo.Pago', N'U') IS NULL
    CREATE TABLE dbo.Pago (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Pago PRIMARY KEY,
        MovimientoId int NOT NULL CONSTRAINT UQ_Pago_Movimiento UNIQUE,
        Metodo int NOT NULL,
        Monto decimal(10,2) NOT NULL,
        Fecha datetime2(0) NOT NULL CONSTRAINT DF_Pago_Fecha DEFAULT SYSDATETIME(),
        CONSTRAINT FK_Pago_Movimiento FOREIGN KEY (MovimientoId) REFERENCES dbo.Movimiento(Id),
        CONSTRAINT CK_Pago_Metodo CHECK (Metodo IN (0,1)),
        CONSTRAINT CK_Pago_Monto CHECK (Monto > 0)
    );

    -- Protegen entradas simultáneas, incluso desde dos cajas.
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Movimiento') AND name = N'UX_Movimiento_VehiculoActivo')
        CREATE UNIQUE INDEX UX_Movimiento_VehiculoActivo ON dbo.Movimiento(VehiculoId) WHERE FechaHoraSalida IS NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Movimiento') AND name = N'UX_Movimiento_EspacioActivo')
        CREATE UNIQUE INDEX UX_Movimiento_EspacioActivo ON dbo.Movimiento(EspacioId) WHERE FechaHoraSalida IS NULL;

    -- Datos exclusivamente de demostración; no sobrescribir cuentas existentes.
    IF NOT EXISTS (SELECT 1 FROM dbo.Empleado WHERE Usuario = N'admin')
        INSERT dbo.Empleado (Nombres, Apellidos, Telefono, Cargo, Usuario, ClaveHash, Activo, Rol)
        VALUES (N'Administrador', N'Demo', N'5555-0100', N'Administrador', N'admin',
            'PBKDF2-SHA256$210000$bNh1YBfcShyUq1oTZuKeMg==$5cBspFfA8UrG2PlLfn38d752Rk5nfw34HhnEdEjvGYk=', 1, 0);

    -- Se agregan ejemplos solo cuando las tablas respectivas están vacías.
    IF NOT EXISTS (SELECT 1 FROM dbo.Cliente)
        INSERT dbo.Cliente (Nombres, Apellidos, Telefono, Membresia)
        VALUES (N'Ana', N'López', N'5555-0101', 1),
               (N'Luis', N'Pérez', N'5555-0102', 0),
               (N'Marta', N'García', N'5555-0103', 0);

    IF NOT EXISTS (SELECT 1 FROM dbo.Vehiculo)
    BEGIN
        DECLARE @ClienteId int = (SELECT MIN(Id) FROM dbo.Cliente);
        INSERT dbo.Vehiculo (Placa, ClienteId, TipoVehiculo, NumeroPuertas, Cilindraje, CapacidadToneladas)
        VALUES (N'P001AAA', @ClienteId, 0, 4, NULL, NULL),
               (N'M001AAA', @ClienteId, 1, NULL, 150, NULL),
               (N'C001AAA', @ClienteId, 2, NULL, NULL, 5.00);
    END;

    IF NOT EXISTS (SELECT 1 FROM dbo.EspacioParqueo)
        INSERT dbo.EspacioParqueo (Numero, Nivel)
        VALUES (1,1), (2,1), (3,1), (4,1), (5,1),
               (1,2), (2,2), (3,2), (4,2), (5,2);

    IF NOT EXISTS (SELECT 1 FROM dbo.Tarifa WHERE TipoVehiculo = 0)
        INSERT dbo.Tarifa (TipoVehiculo, PrecioHora, PrecioDia, PrecioMensualidad) VALUES (0,10,60,600);
    IF NOT EXISTS (SELECT 1 FROM dbo.Tarifa WHERE TipoVehiculo = 1)
        INSERT dbo.Tarifa (TipoVehiculo, PrecioHora, PrecioDia, PrecioMensualidad) VALUES (1,7,42,420);
    IF NOT EXISTS (SELECT 1 FROM dbo.Tarifa WHERE TipoVehiculo = 2)
        INSERT dbo.Tarifa (TipoVehiculo, PrecioHora, PrecioDia, PrecioMensualidad) VALUES (2,15,90,900);

    -- Los movimientos y pagos se crearán al usar entradas y salidas.
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
-- En una base nueva: 1 empleado, 3 clientes, 3 vehículos, 10 espacios,
-- 3 tarifas, 0 movimientos y 0 pagos.
SELECT N'Empleado' AS Tabla, COUNT(*) AS Cantidad FROM dbo.Empleado
UNION ALL SELECT N'Cliente', COUNT(*) FROM dbo.Cliente
UNION ALL SELECT N'Vehiculo', COUNT(*) FROM dbo.Vehiculo
UNION ALL SELECT N'EspacioParqueo', COUNT(*) FROM dbo.EspacioParqueo
UNION ALL SELECT N'Tarifa', COUNT(*) FROM dbo.Tarifa
UNION ALL SELECT N'Movimiento', COUNT(*) FROM dbo.Movimiento
UNION ALL SELECT N'Pago', COUNT(*) FROM dbo.Pago;
GO
