# Gesti-nParqueoSolution

Sistema de Gestión de Parqueo — Proyecto Final de Programación II, UMG.
C# y Windows Forms con SQL Server. Código sencillo para explicar abstracción, herencia y polimorfismo.

## Continuidad y forma de trabajo

Leer primero `MEMORIA.md` y `docs/REQUISITOS_ORIGINALES.txt`. La entrega se divide en ocho etapas y se espera el «continúa» del usuario entre ellas. Este README conserva las etapas anteriores y se amplía al cerrar cada entrega.

## Guía rápida para empezar

1. Instala **Visual Studio 2022** con «Desarrollo de escritorio de .NET», el **SDK .NET 8**, **SQL Server Express o LocalDB** y **SSMS**.
2. En SSMS, con **Autenticación de Windows**, abre y ejecuta completo [el script de la base de datos](BaseDatos/01_ParqueoDB.sql). Comprueba que finaliza sin errores.
3. Abre [la solución en Visual Studio](GestionParqueoSolution.sln) y compílala con **Compilar → Compilar solución**. Si SQL Server no está en LocalDB, ajusta la constante `Servidor` en [Conexion.cs](Backend/LibreriaClases/Datos/Conexion.cs).
4. Para comprobar el backend, configura **PruebasBackend** como proyecto de inicio y ejecuta con **Ctrl+F5**. Debe informar 61 comprobaciones correctas. Estas pruebas locales no requieren conectarse a SQL Server.
5. En Windows, establece **GestionParqueo.UI** como proyecto de inicio y presiona **F5**. Debe aparecer únicamente el formulario de acceso. Después de ejecutar el SQL inicial, inicia sesión con el administrador de demostración indicado en la etapa 1.
6. Las pruebas integradas con SQL están preparadas para Windows; la guía y el comando están en [Pruebas y compilación de la etapa 2](#comandos-de-compilación-y-pruebas). Usa una base de desarrollo.

La etapa 3 ya incluye **FrmPrincipal y UcLogin**. Para abrir el programa, selecciona GestionParqueo.UI como proyecto de inicio en Visual Studio. Consulta [las instrucciones de la etapa 3](#etapa-3--ventana-principal-e-inicio-de-sesión), [todas las instrucciones de la base de datos](#etapa-1--base-de-datos-y-documentación), [las instrucciones de Visual Studio y el backend](#pasos-concretos-en-visual-studio), o la [memoria de continuidad](MEMORIA.md).

## Requisitos

- Windows y Visual Studio 2022 actualizado, con la carga «Desarrollo de escritorio de .NET».
- SDK .NET 8. Los proyectos usarán `net8.0-windows`.
- SQL Server Express o SQL Server LocalDB y SQL Server Management Studio (SSMS).
- Una cuenta de Windows con permiso para crear ParqueoDB y trabajar con ella.
- Más adelante: paquete NuGet `Microsoft.Data.SqlClient` en LibreriaClases. Es la única dependencia de aplicación permitida; no usar Entity Framework ni Dapper.

El entorno de nube es Linux y tiene SDK 8.0.426. Se verificó una consola temporal con salida `Hello, World!`. Windows Forms y LocalDB deben ejecutarse en Windows. El backend y sus pruebas de consola están disponibles desde la etapa 2; la UI se entregará desde la etapa 3.

## Etapa 1 — Base de datos y documentación

El estado de esta sección corresponde a su entrega; los avances posteriores se registran en la etapa 2.

Archivos disponibles:

| Ruta | Finalidad |
|---|---|
| `BaseDatos/01_ParqueoDB.sql` | Crea ParqueoDB, siete tablas, relaciones, restricciones, índices y datos de demostración. Incluye consulta final de conteos. |
| `README.md` | Requisitos, instrucciones y comprobaciones por etapa. |
| `MEMORIA.md` | Estado real, decisiones y próximos pasos para quien continúe. |
| `docs/REQUISITOS_ORIGINALES.txt` | Copia íntegra del encargo original. |

La base contiene Empleado, Cliente, Vehiculo, EspacioParqueo, Tarifa, Movimiento y Pago. Los identificadores son enteros autoincrementales. No se crean usuarios ni logins de SQL Server; la conexión debe usar autenticación de Windows.

### Pasos que debes hacer ahora

1. Instala SQL Server Express o LocalDB y SSMS si todavía no los tienes.
2. Abre SSMS. Usa `(localdb)\MSSQLLocalDB` para LocalDB o `.\SQLEXPRESS` para una instalación Express con ese nombre. Elige **Autenticación de Windows**. El nombre real de tu instancia puede ser distinto.
3. Abre `BaseDatos/01_ParqueoDB.sql` con **Archivo → Abrir → Archivo**. Ejecuta el archivo completo con F5; no selecciones únicamente una parte. `GO` es un separador de lotes que entiende SSMS.
4. Comprueba que aparece el resultado final con siete filas y que no hay errores. Actualiza el explorador de objetos y expande **Bases de datos → ParqueoDB → Tablas**.
5. Ejecuta el archivo completo una segunda vez. En una base nueva y sin cambios entre ejecuciones, los conteos deben mantenerse iguales.
6. Conserva los archivos en sus rutas. En esta etapa aún no se abre ninguna pantalla y no existe un inicio de sesión funcional.

No hace falta crear proyectos para ejecutar esta etapa. En la etapa 2 se indicará cómo crear la solución, las carpetas de solución Backend y Frontend y LibreriaClases. La UI, su referencia a la biblioteca y la selección de proyecto de inicio se documentarán cuando se entreguen esos archivos.

### Qué debes ver y cómo probar

En una base recién creada:

| Tabla | Cantidad |
|---|---:|
| Empleado | 1 |
| Cliente | 3 |
| Vehiculo | 3 |
| EspacioParqueo | 10 |
| Tarifa | 3 |
| Movimiento | 0 |
| Pago | 0 |

Los diez espacios están disponibles, cinco en cada nivel. Los vehículos de ejemplo son `P001AAA` (automóvil de cuatro puertas), `M001AAA` (motocicleta de 150 cc) y `C001AAA` (camión de cinco toneladas). Los tres pertenecen al primer cliente existente. Los demás clientes permiten probar altas y cambios de propietario más adelante.

En SSMS puedes ejecutar estas consultas de lectura:

```sql
USE ParqueoDB;
SELECT Usuario, Activo, Rol FROM dbo.Empleado;
SELECT Placa, TipoVehiculo, NumeroPuertas, Cilindraje, CapacidadToneladas FROM dbo.Vehiculo;
SELECT Nivel, COUNT(*) AS Espacios, SUM(CAST(Disponible AS int)) AS Libres
FROM dbo.EspacioParqueo GROUP BY Nivel;
SELECT TipoVehiculo, PrecioHora, PrecioDia, PrecioMensualidad FROM dbo.Tarifa;
```

La cuenta inicial de **demostración** es `admin`, con contraseña `ParqueoDemo2026!`, activa y con rol Administrador. Es una credencial pública de ejemplo, no una cuenta para producción. La base almacena únicamente su hash. No hay pantalla de login hasta la etapa 3. Reejecutar el script no cambia la contraseña ni reactiva una cuenta existente.

### Contrato para el backend de la etapa 2

Los valores de las enumeraciones deben coincidir con SQL:

| Enumeración | Valores |
|---|---|
| RolUsuario | Administrador = 0, Cajero = 1 |
| MetodoPago | Efectivo = 0, Tarjeta = 1 |
| TipoVehiculo | Automovil = 0, Motocicleta = 1, Camion = 2 |

`ClaveHash` usa `PBKDF2-SHA256$iteraciones$salBase64$hashBase64`: 210000 iteraciones, sal de 16 bytes y resultado de 32 bytes. La clase Seguridad deberá usar PBKDF2-HMAC-SHA256 con contraseña UTF-8, generar una sal aleatoria por contraseña nueva y comparar los resultados con `CryptographicOperations.FixedTimeEquals`. .NET lo ofrece sin paquetes adicionales. La sal del ejemplo está incluida en el hash y no es secreta. No usar SHA256 simple ni comparar directamente contraseñas con ClaveHash.

Tarifas de ejemplo en quetzales: automóvil Q10/h, motocicleta Q7/h y camión Q15/h; los valores diarios son Q60, Q42 y Q90 y las mensualidades Q600, Q420 y Q900. Los precios por tipo ya representan el descuento o recargo: no aplicar dos veces el mismo ajuste. En la etapa 2 se deberá explicar cómo cada override obtiene ese resultado desde la tarifa base y mantener consistencia con las tarifas editables. El encargo no define todavía la prioridad de tarifa diaria o mensual ni el redondeo de fracciones de hora; se deberán documentar esas decisiones al implementar GestorParqueo.

Un movimiento activo tiene FechaHoraSalida NULL. Los índices únicos filtrados impiden que un vehículo o espacio tenga dos movimientos activos. Solo se admite un pago completo por movimiento; no hay pagos parciales. Las claves foráneas conservan el historial: no se permite borrar una entidad referenciada.

GestorParqueo deberá actualizar entrada/salida y disponibilidad en una misma transacción. En una salida también debe registrar el pago en esa transacción. Las restricciones de SQL no sincronizan por sí solas Disponible con Movimiento ni garantizan que un pago corresponda a una salida cerrada: esas reglas quedan pendientes en el backend. Todas las consultas de aplicación usarán parámetros; los INSERT del script son datos de ejemplo constantes.

### Reejecución y límites del script

El script crea objetos ausentes y no borra datos. Inserta admin solo si no existe; agrega cada tarifa ausente y carga clientes, vehículos o espacios de ejemplo solo si su tabla está vacía. No es un sistema de migraciones: si ya existe una tabla con columnas o restricciones diferentes, se debe revisar el esquema antes de ejecutarlo. No utilizarlo sobre una base ajena del mismo nombre.

La creación de la base ocurre antes de la transacción. Si falla una instrucción posterior, se revierte la creación de tablas y datos de ese bloque, pero puede permanecer la base vacía. No hace falta eliminarla para repetir la ejecución tras resolver el error.

### Problemas comunes

| Problema | Qué revisar |
|---|---|
| No se puede conectar | Comprueba el nombre real de la instancia, que SQL Server esté iniciado y que estés usando autenticación de Windows. LocalDB debe estar instalado en tu cuenta de Windows. |
| Permiso denegado al crear la base | Pide al administrador de tu instalación permiso para crear ParqueoDB o que la cree antes de ejecutar el resto. No cambies a autenticación SQL. |
| Error de certificado en SSMS local | Para la instancia local de desarrollo, revisa la opción «Confiar en el certificado del servidor». La futura clase Conexion usará TrustServerCertificate=True. |
| La base no existe | Ejecuta el archivo completo, incluidos USE master, CREATE DATABASE y los separadores GO. |
| Error cerca de GO | Ejecuta con SSMS o sqlcmd. GO no se envía como SQL dentro de un SqlCommand. |
| Ya existe una tabla incompatible | El script no migra esquemas anteriores. Compara el esquema y conserva una copia de tus datos antes de cualquier cambio manual. |
| Error de clave única o referencia | Revisa placas/usuarios duplicados y las relaciones. No borres restricciones para permitir datos inválidos. |
| Más adelante: falta SqlConnection | Instala Microsoft.Data.SqlClient en LibreriaClases y usa su espacio de nombres. |
| Más adelante: la UI no reconoce LibreriaClases | Agrega la referencia de proyecto desde GestionParqueo.UI a LibreriaClases. |

### POO para la exposición

En esta etapa se prepara la persistencia; todavía no hay clases C# que demuestren POO. La tabla Vehiculo conserva el tipo y las columnas particulares de las futuras clases Automovil, Motocicleta y Camion. La abstracción, herencia y polimorfismo se implementarán en las jerarquías Persona y Vehiculo de la etapa 2. Las relaciones SQL no equivalen a herencia de C#.

### Validación y estado real

Se revisaron estáticamente tablas, relaciones, datos, formato del hash y ausencia de órdenes destructivas o creación de logins. Se comprobó el hash PBKDF2 de demostración mediante Python. No se ejecutó el script en SQL Server: no hay una instancia disponible en esta máquina Linux. La prueba funcional y la reejecución en SSMS quedan pendientes en Windows; los conteos anteriores son resultados esperados.

## Siguientes etapas

2. Backend completo.
3. FrmPrincipal y UcLogin.
4. UserControls de información base.
5. Forms modales.
6. UcEntradaSalida y UcPagos.
7. UcReportes.
8. Requerimientos, diagrama de clases en texto y README final.

La etapa 2 fue autorizada con «continúa» y se describe a continuación.


## Etapa 2 — Backend completo

Se creó `GestionParqueoSolution.sln` con las carpetas de solución **Backend**, **Frontend** (todavía vacía) y **Pruebas**. LibreriaClases usa `net8.0-windows`; no utiliza controles de Windows Forms, por lo que sus comprobaciones de modelos pueden ejecutarse también en la nube. La UI todavía no existe.

### Archivos de esta etapa

Todas las rutas de la tabla son relativas a la raíz del repositorio. Los archivos están completos; se pueden abrir directamente o copiar conservando las rutas.

| Ruta | Para qué sirve |
|---|---|
| `GestionParqueoSolution.sln` | Solución para abrir con Visual Studio. |
| `.gitignore` | Evita agregar bin, obj y archivos personales de Visual Studio. |
| `Backend/LibreriaClases/LibreriaClases.csproj` | Biblioteca .NET 8 para Windows; referencia a Microsoft.Data.SqlClient 6.1.7. |
| `Backend/LibreriaClases/packages.lock.json` | Versiones e integridad de las dependencias resueltas. |
| `Backend/LibreriaClases/Enumeraciones/RolUsuario.cs` | Administrador y Cajero. |
| `Backend/LibreriaClases/Enumeraciones/MetodoPago.cs` | Efectivo y Tarjeta. |
| `Backend/LibreriaClases/Enumeraciones/TipoVehiculo.cs` | Automovil, Motocicleta y Camion. |
| `Backend/LibreriaClases/Modelos/Validacion.cs` | Validaciones pequeñas compartidas de texto, identificadores y precios. |
| `Backend/LibreriaClases/Modelos/Persona.cs` | Clase abstracta con campos privados y datos comunes. |
| `Backend/LibreriaClases/Modelos/Cliente.cs` | Persona con membresía y fecha de registro. |
| `Backend/LibreriaClases/Modelos/Empleado.cs` | Persona con usuario, hash, cargo, estado y rol. |
| `Backend/LibreriaClases/Modelos/Vehiculo.cs` | Clase abstracta; propietario y contrato de cálculo de tarifa. |
| `Backend/LibreriaClases/Modelos/Automovil.cs` | Calcula usando la tarifa base. |
| `Backend/LibreriaClases/Modelos/Motocicleta.cs` | Calcula restando un descuento a la tarifa base por hora. |
| `Backend/LibreriaClases/Modelos/Camion.cs` | Calcula sumando un recargo a la tarifa base por hora. |
| `Backend/LibreriaClases/Modelos/EspacioParqueo.cs` | Número, nivel y disponibilidad. |
| `Backend/LibreriaClases/Modelos/Tarifa.cs` | Precios positivos con dos decimales y tipo de vehículo. |
| `Backend/LibreriaClases/Modelos/Movimiento.cs` | Entrada, salida nullable y duración calculada. |
| `Backend/LibreriaClases/Modelos/Pago.cs` | Movimiento, método, monto y fecha. |
| `Backend/LibreriaClases/Datos/Conexion.cs` | Única cadena de conexión y constante Servidor. |
| `Backend/LibreriaClases/Datos/DatosSql.cs` | Parámetros tipados, ejecución y envoltura de transacciones T-SQL. |
| `Backend/LibreriaClases/Datos/ClienteRepositorio.cs` | CRUD de clientes y búsqueda por nombre, apellido o teléfono. |
| `Backend/LibreriaClases/Datos/EmpleadoRepositorio.cs` | CRUD de empleados y búsqueda por usuario para el login. |
| `Backend/LibreriaClases/Datos/VehiculoRepositorio.cs` | CRUD de vehículos; reconstruye la clase hija correcta y carga sus tarifas. |
| `Backend/LibreriaClases/Datos/EspacioParqueoRepositorio.cs` | CRUD de espacios y listado de espacios libres. |
| `Backend/LibreriaClases/Datos/TarifaRepositorio.cs` | CRUD de tarifas y aplicación de precios a cada vehículo. |
| `Backend/LibreriaClases/Datos/MovimientoRepositorio.cs` | Operaciones de movimientos, ocupación por hora y clientes recurrentes. |
| `Backend/LibreriaClases/Datos/PagoRepositorio.cs` | Operaciones de pagos e ingresos por día y mes. |
| `Backend/LibreriaClases/Logica/Seguridad.cs` | PBKDF2 con sal aleatoria y comparación de tiempo fijo. |
| `Backend/LibreriaClases/Logica/Sesion.cs` | Login, cierre de sesión, empleado actual y comprobación de permisos. |
| `Backend/LibreriaClases/Logica/GestorParqueo.cs` | Entradas, cotización, salida, pago y disponibilidad en transacciones. |
| `Pruebas/PruebasBackend/PruebasBackend.csproj` | Consola de comprobación sin paquetes adicionales de pruebas. |
| `Pruebas/PruebasBackend/packages.lock.json` | Dependencias fijadas de la consola de comprobación. |
| `Pruebas/PruebasBackend/Program.cs` | Pruebas locales y modo opcional de integración SQL. |
| `README.md` y `MEMORIA.md` | Instrucciones acumulativas, estado y continuidad. |

### Pasos concretos en Visual Studio

1. Conserva la estructura de carpetas entregada. Abre `GestionParqueoSolution.sln` con Visual Studio 2022. No crees otra solución encima de esta.
2. Comprueba que **Backend** contiene **LibreriaClases** y que **Frontend** está vacía. **Pruebas** contiene **PruebasBackend**. No elimines la carpeta Frontend: se usará en la etapa 3.
3. Permite la restauración de NuGet. La referencia directa de la aplicación es Microsoft.Data.SqlClient, fijada en 6.1.7; las dependencias transitivas son instaladas por NuGet. No instales EF, Dapper ni paquetes de pruebas.
4. Ejecuta el SQL de la etapa 1 en SSMS con autenticación de Windows, si todavía no lo hiciste.
5. Abre `Backend/LibreriaClases/Datos/Conexion.cs`. Deja `Servidor = @"(localdb)\MSSQLLocalDB"` para LocalDB o cambia exclusivamente esa constante por `@".\SQLEXPRESS"` si ese es tu servidor. No agregues usuario ni contraseña SQL. La clase ya usa Integrated Security=True y TrustServerCertificate=True.
6. Selecciona **Compilar → Compilar solución**. Debe terminar sin errores. No intentes iniciar LibreriaClases: es una biblioteca.
7. Para comprobar esta etapa, establece **PruebasBackend** como proyecto de inicio y usa **Ctrl+F5**. Debes ver `Correctas: 61 comprobaciones` y un aviso explícito de que SQL Server no se ejecutó. Estas pruebas no necesitan conexión.
8. Para las pruebas reales de SQL, usa una copia de desarrollo de ParqueoDB. Desde una terminal en la raíz, ejecuta los comandos del apartado siguiente con `--sql`. Necesitan el SQL de la etapa 1 y una cuenta administradora de la aplicación.
9. GestionParqueo.UI se creará en la etapa 3; entonces se agregará su referencia a LibreriaClases y se marcará como proyecto de inicio. En esta etapa no hay formularios ni pantalla de login.

Si estás reconstruyendo manualmente en lugar de abrir los archivos entregados: crea una solución en blanco llamada GestionParqueoSolution; agrega las carpetas de solución Backend y Frontend; crea la biblioteca LibreriaClases dentro de Backend; reemplaza su `.csproj` por el entregado, elimina Class1.cs si Visual Studio la generó y copia Enumeraciones, Modelos, Datos y Logica con sus archivos. La solución ya entregada incluye también la consola de pruebas, de modo que abrirla evita estos pasos manuales.

### Comandos de compilación y pruebas

En la raíz, con el SDK .NET 8 disponible:

```shell
dotnet restore GestionParqueoSolution.sln --locked-mode
dotnet build GestionParqueoSolution.sln --configuration Release --no-restore
dotnet run --project Pruebas/PruebasBackend --configuration Release --no-build
```

En Windows, para añadir la integración con SQL Server:

```shell
dotnet run --project Pruebas/PruebasBackend --configuration Release --no-build -- --sql
```

El modo SQL usa por defecto el usuario de aplicación `admin` y su contraseña pública de demostración de la etapa 1. Si cambiaste esa cuenta, puedes proporcionar `PARQUEO_PRUEBA_USUARIO` y `PARQUEO_PRUEBA_CLAVE` mediante variables locales de proceso; nunca las agregues al repositorio ni a la cadena de conexión. Son credenciales del empleado dentro de la aplicación, no credenciales de SQL Server. El acceso al servidor sigue utilizando tu cuenta de Windows.

Las pruebas SQL crean registros identificados con E2 y una marca aleatoria: cliente, vehículos de las tres clases, espacios y cajero. Comprueban login, restricciones por rol, entrada, duplicados, monto incorrecto, salida y pago, consultas de reportes, anulación y liberación. Un bloque finally intenta retirar solo los identificadores creados por esa ejecución; si se interrumpe el proceso o falla la limpieza, revisa esos registros en la base de desarrollo. No ejecutar sobre datos operativos.

Para usar el SDK de la nube:

```shell
export PATH=/workspace/.dotnet:$PATH
export DOTNET_CLI_HOME=/workspace/.dotnet-cli
export NUGET_PACKAGES=/workspace/.nuget/packages
```

### Reglas implementadas y decisiones explicables

- **Abstracción:** no se pueden crear Persona ni Vehiculo directamente. Sus contratos obligan a implementar ObtenerResumen y CalcularTarifa.
- **Encapsulación:** Persona y Vehiculo protegen sus datos con campos privados y validaciones. Id=0 significa pendiente de guardar; las operaciones SQL requieren identificadores positivos.
- **Herencia:** Cliente y Empleado reutilizan Persona; Automovil, Motocicleta y Camion reutilizan Vehiculo.
- **Polimorfismo:** al recorrer List<Persona>, ObtenerResumen usa el override real. GestorParqueo llama CalcularTarifa a una referencia Vehiculo y se ejecuta el override de su hija. VehiculoRepositorio mantiene la clase hija al leer SQL.
- **Tarifas:** TarifaBaseHora proviene de la tarifa de automóvil. Para moto, DescuentoHora = base menos precio de moto; para camión, RecargoHora = precio de camión menos base. Con los datos iniciales: 10−3=7 y 10+5=15 por hora. No se aplica un segundo ajuste. Cambiar los precios en SQL o en el futuro módulo de tarifas altera las cotizaciones nuevas; Monto de pagos ya guardados no se recalcula.
- **Orden de precios:** el precio por hora de moto no puede superar el de automóvil y el de camión no puede ser inferior. Un tipo de tarifa existente no se cambia; se editan sus precios. No se elimina una tarifa necesaria para vehículos registrados.
- **Duración:** se guarda en segundos completos; cualquier fracción de hora inicia otra hora de cobro, con mínimo de una. Se cobran todas las horas: por ejemplo, 25 horas usan 25 veces el precio por hora. PrecioDia y PrecioMensualidad se gestionan pero no se aplican automáticamente, porque el encargo no define pases diarios ni contratos de mensualidad. Membresia no da un descuento adicional implícito.
- **Integridad:** una entrada ocupa el espacio en la misma transacción. Una salida guarda el cierre, el único pago completo y la liberación en otra transacción. Los bloqueos y los índices únicos respaldan las comprobaciones ante operaciones concurrentes. Si falla una instrucción, se revierte la operación completa.
- **Cotización:** PrepararSalida devuelve un Pago todavía sin Id, con fecha y monto calculados. Para confirmar, se envían ese monto y esa fecha a RegistrarSalida, junto al método elegido. Si la entrada o las tarifas cambiaron entre ambas llamadas, se rechaza y se debe cotizar de nuevo. No se aceptan montos distintos del calculado ni fechas futuras. Las fechas usan la hora local de Windows; mantener cliente y servidor con reloj y zona horaria coherentes.
- **Permisos:** iniciar sesión exige empleado activo y hash válido. Un login fallido limpia la sesión anterior. Solo Administrador modifica información base o realiza correcciones/anulaciones; Cajero puede consultar y registrar entradas y salidas. Las próximas pantallas deberán respetar esos permisos y no mostrar ClaveHash en tablas.

### Alcance del CRUD de movimientos y pagos

Cada repositorio tiene Listar, ObtenerPorId, Insertar, Actualizar y Eliminar. En movimientos y pagos los métodos respetan las reglas del parqueo:

- Movimiento.Insertar registra una entrada activa mediante GestorParqueo. Actualizar corrige únicamente la entrada de una visita aún activa, sin cambiar vehículo o espacio y sin solapar historial. Eliminar cancela una entrada activa sin pago y libera el espacio.
- Pago.Insertar pasa por RegistrarSalida: no crea un pago separado del cierre. Actualizar corrige únicamente el método del pago; el monto, fecha y movimiento deben coincidir con lo guardado. Eliminar anula el pago y reabre su movimiento, ocupando de nuevo el espacio, únicamente si no existe otra visita activa ni una visita posterior del vehículo o del espacio. La próxima pantalla de pagos no tiene que ofrecer estas operaciones administrativas si no están pedidas por el encargo.
- Las claves foráneas impiden borrar clientes, vehículos o espacios con historial. No hay eliminación en cascada. Los cambios de tipo de un vehículo con historial también se rechazan.

Las consultas de ocupación toman una foto al inicio de cada hora del día elegido. Las horas futuras no aparecen; no se cuentan todas las visitas que pasaron dentro de la hora. Capacidad es el número actual de espacios, no un histórico de capacidad. Los reportes de ingresos usan Fecha del pago y rangos `[desde, hasta)`; clientes recurrentes cuenta las entradas dentro de ese rango y agrupa por propietario actual del vehículo. Todavía no existe la interfaz de reportes.

### Problemas comunes añadidos en esta etapa

| Problema | Solución |
|---|---|
| No se puede ejecutar LibreriaClases | Es una biblioteca. Para esta etapa selecciona PruebasBackend; desde etapa 3 se iniciará GestionParqueo.UI. |
| Falta el SDK net8.0-windows | Instala .NET 8 y actualiza Visual Studio 2022; conserva el framework del `.csproj`. |
| Error de restauración en modo bloqueado | Usa los packages.lock.json entregados y el `.csproj` correspondiente. Si cambias dependencias deliberadamente, restaura para actualizar los locks y revisa el cambio; no borres verificaciones para ocultarlo. |
| Login rechazado | Comprueba la cuenta de aplicación, Activo, Rol y el formato del hash; ejecutar otra vez el SQL no restablece contraseñas existentes. |
| Falta o es incoherente una tarifa | Deben existir la base de automóvil y la del tipo consultado; revisa que moto <= automóvil <= camión. |
| Entrada rechazada | Actualiza los listados: puede haber otro movimiento activo, un espacio ocupado o una fecha que se solapa con historial. |
| No se puede corregir o anular un movimiento/pago | Comprueba rol y estado. Los métodos protegen el historial y no permiten dejar pagos o disponibilidad incoherentes. |
| Error de interbloqueo o datos cambiados | La transacción se revierte; actualiza los datos y vuelve a intentar la operación. No se hacen reintentos automáticos ocultos. |

### Validación de la etapa 2

La solución compiló en Release con cero errores y advertencias; se ejecutaron 61 comprobaciones de modelos, polimorfismo, precios, redondeo, hash y permisos sin sesión. Las comprobaciones terminaron con código de salida 0. El modo `--sql` está preparado pero NO se ejecutó: no hay instancia compatible con la autenticación de Windows en esta máquina. Quedan pendientes las pruebas de los repositorios, las transacciones y los reportes en SQL Server. No hay UI para validar en esta etapa.

La auditoría transitiva de Microsoft.Data.SqlClient 6.1.7 reportó `System.Security.Cryptography.Pkcs` 8.0.1 con avisos [GHSA-846f-f764-76mx](https://github.com/advisories/GHSA-846f-f764-76mx) (alto) y [GHSA-2fmv-rv2c-59x7](https://github.com/advisories/GHSA-2fmv-rv2c-59x7) (moderado). Actualizar SqlClient dentro de la serie 6.1 no eliminó esos avisos. El usuario confirmó «Mantener únicamente Microsoft.Data.SqlClient»: no se agregará Pkcs como referencia directa. Los avisos continúan pendientes y se conserva la restricción de un único paquete directo de aplicación. No se desactivó la auditoría. Compilar correctamente no equivale a una auditoría sin avisos.

### Resumen para exponer (máximo cinco líneas)

Persona y Vehiculo son abstracciones que reúnen lo común y obligan a sus hijas a completar comportamientos.
Cliente/Empleado y Automovil/Motocicleta/Camion demuestran herencia y sobrescritura de métodos.
Una misma llamada a CalcularTarifa produce resultados diferentes según la clase real: eso es polimorfismo.
Los repositorios guardan los objetos y GestorParqueo coordina entradas, salidas y pagos sin dejar cambios a medias.
Las contraseñas se verifican con hash y la sesión limita las operaciones según el rol.

La etapa 3 se autorizó con «continúa» y se describe a continuación.


## Etapa 3 — Ventana principal e inicio de sesión

La solución ahora incluye una aplicación Windows Forms, `GestionParqueo.UI`. Se entrega `FrmPrincipal` con menú y panel contenedor, y `UcLogin` para iniciar sesión. La aplicación referencia a LibreriaClases. Se usaron controles estándar sin estilos personalizados.

### Archivos de esta etapa

| Ruta | Para qué sirve |
|---|---|
| `GestionParqueoSolution.sln` | Incluye GestionParqueo.UI dentro de la carpeta de solución Frontend. |
| `Frontend/GestionParqueo.UI/GestionParqueo.UI.csproj` | Proyecto Windows Forms net8.0-windows; referencia LibreriaClases sin paquetes directos adicionales. |
| `Frontend/GestionParqueo.UI/packages.lock.json` | Versiones e integridad de las dependencias resueltas. |
| `Frontend/GestionParqueo.UI/Program.cs` | Punto de entrada que abre FrmPrincipal. |
| `Frontend/GestionParqueo.UI/FrmPrincipal.cs` | MenuStrip, Panel contenedor, CargarControl, visibilidad por rol y cierre de sesión. |
| `Frontend/GestionParqueo.UI/Controles/UcLogin.cs` | Usuario, contraseña oculta, validación y acceso mediante Sesion.Iniciar. |
| `Frontend/GestionParqueo.UI/Forms/.gitkeep` | Reserva la carpeta Forms para los formularios modales de la etapa 5. |
| `README.md` y `MEMORIA.md` | Instrucciones acumulativas y estado para continuar. |

### Pasos concretos en Visual Studio

1. Instala Visual Studio 2022 con «Desarrollo de escritorio de .NET» y el SDK .NET 8. Conserva las rutas de los archivos entregados.
2. Ejecuta completo `BaseDatos/01_ParqueoDB.sql` en SSMS con **Autenticación de Windows**, si todavía no lo hiciste. Comprueba que existe ParqueoDB y el empleado `admin`.
3. Abre `GestionParqueoSolution.sln`. En el Explorador de soluciones verás **Backend → LibreriaClases** y **Frontend → GestionParqueo.UI**. La referencia de UI a LibreriaClases ya está agregada en el `.csproj`.
4. Si tu instancia no es LocalDB, abre `Backend/LibreriaClases/Datos/Conexion.cs` y cambia la constante `Servidor` por el nombre real, por ejemplo `@".\SQLEXPRESS"`. La conexión seguirá usando Integrated Security=True y TrustServerCertificate=True.
5. Permite restaurar los paquetes NuGet y usa **Compilar → Compilar solución**. La única referencia directa de paquete de la aplicación sigue siendo Microsoft.Data.SqlClient en LibreriaClases.
6. Haz clic derecho en **GestionParqueo.UI → Establecer como proyecto de inicio**. Presiona **F5**. Se abrirá la ventana «Gestión de Parqueo» con el control de login y el menú oculto.
7. Escribe `admin` y la contraseña pública de demostración `ParqueoDemo2026!`. Tras el acceso verás el saludo y el menú. Usa **Sesión → Cerrar sesión**: debe volver a aparecer el login y desaparecer el menú.

Para construir desde terminal:

```shell
dotnet restore GestionParqueoSolution.sln --locked-mode
dotnet build GestionParqueoSolution.sln --configuration Release --no-restore
```

En Windows, con la base preparada, también puedes iniciar la interfaz mediante `dotnet run --project Frontend/GestionParqueo.UI --configuration Release --no-build`.

### Qué debe verse y cómo comprobarlo

- Al abrir, solo aparece el acceso. El menú no se muestra antes de una sesión válida.
- Si usuario o contraseña están vacíos, se pide completarlos sin intentar consultar la base. Si son incorrectos o el empleado está inactivo, aparece un aviso y continúa el login.
- Con `admin`, el menú muestra Información Base (Clientes, Vehículos, Espacios y Tarifas), Movimientos (Entradas y salidas, Pagos), Consultas y Reportes (Reportes) y Sesión (Cerrar sesión).
- Con una cuenta Cajero activa, el menú muestra Clientes y Vehículos en Información Base, Entradas y salidas, Pagos, Reportes y Cerrar sesión. No aparecen Espacios ni Tarifas. Crear una cuenta Cajero queda para una etapa posterior; no usar contraseñas en texto plano en SQL para probarlo.
- Las entradas de menú de módulos futuros se ven deshabilitadas hasta que existan sus pantallas. Cerrar sesión ya funciona. Los repositorios también exigen rol Administrador para las modificaciones de información base.
- Si falla la conexión, se muestra el error del servidor para poder comprobar la instancia de SQL Server y la configuración de Windows.

`UcLogin` genera un evento cuando Sesion.Iniciar acepta al empleado. `FrmPrincipal` reemplaza el control del panel con CargarControl(UserControl uc), revela el menú según el rol y muestra el nombre de la persona. Al cerrar, Sesion.Cerrar borra el empleado actual y FrmPrincipal vuelve a cargar UcLogin.

### Problemas comunes añadidos

| Problema | Qué revisar |
|---|---|
| La UI no inicia en Visual Studio | Marca GestionParqueo.UI como proyecto de inicio; LibreriaClases es una biblioteca. Ejecuta en Windows, con la carga de Windows Forms instalada. |
| Error al conectar durante el login | Revisa el nombre de `Servidor` en Conexion.cs, que la instancia esté encendida, que ParqueoDB exista y que tu cuenta de Windows tenga acceso. |
| El administrador de ejemplo no entra | Ejecuta el SQL de la etapa 1, verifica `Activo=1` para `admin` y usa la contraseña de demostración. Reejecutar el SQL no reemplaza un hash ya existente. |
| La UI no encuentra LibreriaClases | Comprueba la referencia de proyecto en GestionParqueo.UI.csproj y restaura NuGet. |
| Las opciones aparecen deshabilitadas | Las pantallas de esos módulos corresponden a etapas posteriores. En esta etapa funciona login, visibilidad del menú y cierre de sesión. |

### POO para la exposición

FrmPrincipal depende del control base `UserControl`: CargarControl recibe ese tipo y puede mostrar UcLogin ahora y otros controles en etapas posteriores. Sesion.Iniciar recupera un Empleado, que hereda de Persona. Se reutiliza el backend de la etapa 2 sin copiar la lógica de contraseñas en la interfaz.

### Validación y límites de la etapa 3

En este entorno Linux, `dotnet restore GestionParqueoSolution.sln --use-lock-file` y `dotnet build GestionParqueoSolution.sln --configuration Release --no-restore` terminaron correctamente con **cero errores y cero advertencias**, incluida la aplicación WinForms. Esto comprueba compilación, no la ejecución de la ventana. El arranque, el login real contra SQL Server y el cierre de sesión requieren la comprobación en Windows descrita arriba. La restricción de un único paquete directo permanece; los avisos transitivos de NuGet señalados en la etapa 2 siguen pendientes por decisión del usuario.

### Resumen para exponer (máximo cinco líneas)

FrmPrincipal muestra un UserControl dentro de un panel y permite reemplazarlo sin crear otra ventana principal.
UcLogin pide usuario y contraseña, y Sesion.Iniciar verifica el hash en el backend.
El menú se oculta hasta iniciar sesión y presenta las opciones permitidas según el rol.
Cerrar sesión limpia el empleado actual y vuelve a mostrar el control de acceso.
La interfaz reutiliza la clase Empleado, hija de Persona, y deja preparada la carga de otros controles.

La siguiente entrega es la etapa 4: controles de Clientes, Vehículos, Espacios y Tarifas. Se espera «continúa» antes de implementarla.
