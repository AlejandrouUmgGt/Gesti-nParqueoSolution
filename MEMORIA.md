# Memoria de continuidad — Gestión de Parqueo

Actualización: 22 de octubre de 2026.

## Leer al retomar

1. Leer esta memoria y `docs/REQUISITOS_ORIGINALES.txt`, que conserva el encargo completo.
2. Revisar `git status --short`, el README y los archivos reales antes de asumir avances.
3. Usar el checkout existente; cada tarea de nube ya está aislada. No crear worktrees salvo petición explícita.
4. Trabajar por etapas y esperar el «continúa» del usuario entre ellas. No implementar etapas futuras automáticamente.
5. Al terminar una etapa, actualizar README sin borrar el historial, registrar aquí archivos, pruebas y pendientes, y entregar archivos completos con sus rutas y un resumen de máximo cinco líneas.

## Objetivo y reglas que deben conservarse

Proyecto final de Programación II, Ingeniería en Sistemas, UMG. Aplicación C# sencilla, explicable y comentada en español. Windows Forms, .NET 8 (`net8.0-windows`) y SQL Server. La abstracción, herencia y polimorfismo deben ser visibles.

- Solución GestionParqueoSolution; carpetas de solución Backend y Frontend; proyectos LibreriaClases y GestionParqueo.UI, con referencia de UI a biblioteca.
- Biblioteca: Modelos, Enumeraciones, Datos y Logica. UI: Forms y Controles.
- Única dependencia de aplicación permitida: Microsoft.Data.SqlClient. Sin EF ni Dapper. Consultas parametrizadas; contraseñas de empleados con hash.
- SQL Server con autenticación de Windows exclusivamente. Una clase Conexion concentra el servidor y la cadena, con Integrated Security=True y TrustServerCertificate=True. No crear logins SQL ni inventar credenciales.
- Persona abstracta → Cliente y Empleado; Vehiculo abstracto → Automovil, Motocicleta y Camion. Respetar propiedades, métodos abstractos y virtuales indicados en el encargo completo.
- Repositorios por entidad; reglas de entrada, salida, tarifas, pagos y liberación de espacio en GestorParqueo. Sesion conserva al empleado actual.
- UI con controles estándar y aspecto predeterminado. Sin temas ni decoración elaborada. Administrador ve todo; Cajero tiene las restricciones del encargo.
- README en español: requisitos, archivos por etapa, pasos manuales concretos, pruebas, problemas comunes y explicación de POO.

## Etapas y estado

| Etapa | Entrega | Estado |
|---|---|---|
| 1 | Script SQL completo, datos de ejemplo y README inicial | Entregada; ejecución en SQL Server pendiente |
| 2 | Backend completo | Entregado y compilado; 61 comprobaciones locales correctas; integración SQL pendiente |
| 3 | FrmPrincipal y UcLogin | Entregada y compilada; ejecución Windows/SQL pendiente |
| 4 | UserControls de Clientes, Vehículos, Espacios y Tarifas | Pendiente |
| 5 | Forms modales | Pendiente |
| 6 | UcEntradaSalida y UcPagos | Pendiente |
| 7 | UcReportes | Pendiente |
| 8 | Requerimientos, diagrama de clases en texto y README final | Pendiente |

## Estado observado al iniciar

El commit inicial es b78eb5c2269e1a3f44325750ab9e272e0ddafd70. Solo existía README.md con el nombre del repositorio. No había solución, proyectos, SQL ni pruebas. No declarar ninguna etapa terminada por haber preparado el entorno.

La sesión inicial de preparación añadió esta memoria y una copia íntegra del encargo en docs/REQUISITOS_ORIGINALES.txt. El README original se conserva durante la preparación del entorno.

## Entorno y límites de validación

La máquina de nube es Linux. Windows Forms no se ejecuta aquí y LocalDB requiere Windows. No cambiar la autenticación exigida para sortear este límite. La prueba integral debe hacerse en Windows con Visual Studio, .NET 8 y SQL Server/LocalDB.

La instalación del SDK se guarda fuera del repositorio, en /workspace/.dotnet. Para usarla: `export PATH=/workspace/.dotnet:$PATH`. Verificar `dotnet --info` antes de compilar. Cuando existan proyectos Windows, investigar el uso de `-p:EnableWindowsTargeting=true` para compilación cruzada sin modificar los proyectos durante onboarding; eso no valida la ejecución de la UI.

Al finalizar la etapa 2 sí existen solución, biblioteca y consola de pruebas. Los comandos y resultados están en README. No se ha ejecutado SQL ni validado el login contra la base, la persistencia de pagos o los reportes SQL. Las tarifas de modelos y el hash sí tienen pruebas locales.

## Próximo paso de desarrollo

Esperar «continúa» antes de comenzar la etapa 4: implementar UcClientes, UcVehiculos, UcEspacios y UcTarifas, sin adelantar formularios modales de la etapa 5. FrmPrincipal mantiene las opciones visibles pero deshabilitadas hasta conectar cada módulo. Cajero debe poder consultar Clientes/Vehículos sin modificar; Administrador gestiona información base. Mantener la validación de rol en backend aunque la UI oculte botones.

## Registro que debe completar cada etapa

Anotar fecha, archivos creados o modificados, decisiones y motivo, comandos realmente ejecutados, resultados y pruebas pendientes en Windows. Distinguir implementado, compilado y probado funcionalmente. No almacenar secretos ni marcar pruebas no ejecutadas como exitosas.

## Resultado de preparación del SDK

SDK 8.0.426 instalado y `dotnet --info` correcto. Definir también `DOTNET_CLI_HOME=/workspace/.dotnet-cli` y `NUGET_PACKAGES=/workspace/.nuget/packages`, porque el directorio personal es de solo lectura. El aislamiento impidió crear el mutex NuGet-Migrations; la ejecución autorizada fuera de ese aislamiento permitió crear el proyecto temporal de consola. Este detalle del entorno puede exigir la misma ejecución autorizada al restaurar o compilar. No desactivar comprobaciones de seguridad ni modificar código para evitarlo.

Prueba confirmada: el proyecto temporal /tmp/parqueo-sdk-check se compiló y ejecutó con salida `Hello, World!` y código de salida 0. Esto valida el SDK, no la aplicación. Se guardaron install_script y start_skill en el borrador del entorno; falta que el usuario revise, guarde y publique desde la configuración. No se ha validado restauración en una tarea nueva.

## Etapa 1 — Entrega del 22 de octubre de 2026

- Creado `BaseDatos/01_ParqueoDB.sql`: base ParqueoDB, siete tablas, claves y restricciones, índices únicos filtrados para movimientos activos, carga inicial no destructiva y consulta de conteos.
- Ampliado `README.md`, conservando el título original: pasos SSMS, datos y credenciales exclusivamente de demostración, problemas comunes, conceptos POO y contrato del backend.
- Administrador demo: usuario admin; contraseña pública de ejemplo documentada en README. Solo se almacena hash PBKDF2-SHA256, 210000 iteraciones, sal de 16 bytes, resultado de 32 bytes; formato `PBKDF2-SHA256$iteraciones$salBase64$hashBase64`. Las nuevas contraseñas deben usar sales aleatorias. Seguridad debe coincidir con este formato.
- Enumeraciones: Administrador/Efectivo/Automovil = 0; Cajero/Tarjeta/Motocicleta = 1; Camion = 2.
- Un pago por movimiento, sin pagos parciales. Vehiculo usa ClienteId; Movimiento usa VehiculoId y EspacioId; Pago usa MovimientoId. Identificadores identity. Fechas datetime2(0); dinero decimal.
- Tarifas iniciales por hora: 10/7/15. El backend debe explicar descuento y recargo sin aplicarlos dos veces sobre el precio del tipo. Quedan por definir en etapa 2 redondeo de horas y uso de precio diario/mensual, preservando tarifas editables.
- GestorParqueo deberá usar una transacción para entrada y disponibilidad, y otra para salida/pago/liberación. SQL no sincroniza Disponible automáticamente. No hay borrado en cascada.
- Revisión estática ejecutada mediante Python: siete tablas distintas y ausencia de DROP/TRUNCATE/CREATE LOGIN/CREATE USER. Se verificó el hash con contraseña correcta e incorrecta. Estas comprobaciones NO validan sintaxis ni ejecución SQL Server.
- Pendiente: ejecutar completo en SSMS sobre una base nueva, comprobar conteos (1,3,3,10,3,0,0) y repetir sin duplicados. No hay instancia SQL Server ni UI ejecutable aquí. Etapa 1 entregada como archivos; validación funcional pendiente.

## Etapa 2 — Backend entregado el 22 de octubre de 2026

- Solución `GestionParqueoSolution.sln`, carpetas de solución Backend y Frontend (vacía), biblioteca `Backend/LibreriaClases` net8.0-windows y consola `Pruebas/PruebasBackend`. Solo referencia directa de aplicación: Microsoft.Data.SqlClient 6.1.7. Hay packages.lock.json en ambos proyectos y .gitignore para salidas locales.
- Modelos completos: Persona abstracta con Cliente/Empleado; Vehiculo abstracto con Automovil/Motocicleta/Camion; EspacioParqueo, Tarifa, Movimiento y Pago. Validacion es una ayuda pequeña para propiedades. El README lista todos los archivos con rutas y finalidad.
- Siete repositorios con operaciones CRUD, lectura parametrizada, reconstrucción de hijas y consultas para reportes. DatosSql construye SqlParameter tipados y reutiliza ejecución; no hay EF, Dapper, reflexión, repositorios genéricos ni async.
- Conexion centraliza la cadena; Servidor predeterminado `(localdb)\MSSQLLocalDB`, Integrated Security=True y TrustServerCertificate=True. No hay usuario/contraseña SQL. No se cambió el script SQL de la etapa 1.
- Seguridad usa el formato PBKDF2 de la etapa 1 y compara en tiempo fijo. Sesion valida activo y hash, limpia sesión al fallar y exige rol para mutaciones administrativas. No mostrar ClaveHash en futuros grids.
- GestorParqueo: RegistrarEntrada devuelve Id. PrepararSalida devuelve Pago sin guardar con monto/fecha. RegistrarSalida recibe Id de movimiento, método, monto y opcionalmente la fecha cotizada; devuelve Pago guardado. Se vuelve a comprobar estado y precio antes del cierre.
- Hora iniciada con mínimo de una; segundos completos, sin tarifa diaria/mensual automática ni descuento extra por Membresia. Vehiculo.TarifaBaseHora se carga del automóvil; moto.DescuentoHora=base−precioMoto y camion.RecargoHora=precioCamion−base. No hay doble ajuste. Se rechaza precioMoto>base o precioCamion<base. Los objetos recién construidos tienen valores demostrativos 10/3/5; los obtenidos de repositorios usan tarifas SQL vigentes. Monto histórico de Pago no se recalcula.
- Entradas y salidas usan transacciones T-SQL con XACT_ABORT y TRY/CATCH, sin agregar SqlTransaction al acceso permitido. La operación de salida incluye cierre, pago y disponibilidad. Se usan bloqueos y se rechazan tarifas o entrada cambiadas entre lectura y confirmación.
- CRUD condicionado: Movimiento.Actualizar solo corrige hora de entrada activa; Eliminar cancela entrada activa y libera espacio. Pago.Insertar realiza la salida; Actualizar solo corrige método; Eliminar anula salida/pago y reocupa el espacio si no hay visita posterior o activa que lo impida. Solo Administrador hace correcciones/anulaciones. No cambiar estas reglas desde la UI con SQL directo.
- OcupacionPorHora toma foto al inicio de cada hora (excluye futuras); capacidad actual. IngresosPorDia/Mes y ClientesRecurrentes usan rangos con inicio inclusivo y fin exclusivo. Recurrentes usa propietario actual. La interfaz de reportes sigue pendiente.

### Evidencia y límites

`dotnet restore GestionParqueoSolution.sln --locked-mode` correcto. `dotnet build GestionParqueoSolution.sln --configuration Release --no-restore`: cero errores y cero advertencias. `dotnet run --project Pruebas/PruebasBackend --configuration Release --no-build`: 61 comprobaciones correctas, código 0, mensaje SQL NO EJECUTADO. La suite cubre validaciones, overrides, ajustes configurables, redondeo, hash inicial/correcto/incorrecto/malformado y permisos sin sesión. `git diff --check` sin errores. No se han ejecutado pruebas SQL ni de concurrencia real ni de UI.

Modo `--sql` preparado para Windows con base de desarrollo y administrador: crea y limpia únicamente registros E2 propios; verifica CRUD representativo, roles, duplicados, pago, salida, reportes y anulaciones. Credenciales por defecto son las públicas de demostración de etapa 1; variables opcionales PARQUEO_PRUEBA_USUARIO/CLAVE para pruebas locales, nunca guardar valores. Si se interrumpe, revisar registros E2; la limpieza finally es un intento y puede fallar si se pierde acceso.

Auditoría real: `dotnet list Backend/LibreriaClases/LibreriaClases.csproj package --vulnerable --include-transitive` reporta System.Security.Cryptography.Pkcs 8.0.1 transitivo, severidades High y Moderate, avisos GHSA-846f-f764-76mx y GHSA-2fmv-rv2c-59x7. Actualizar SqlClient de 6.1.3 a 6.1.7 no eliminó esa dependencia. Se consultó al usuario de forma asíncrona si autoriza fijar una dependencia directa corregida; posteriormente el usuario respondió «Mantener únicamente Microsoft.Data.SqlClient». Decisión confirmada: no agregar referencias directas a Pkcs ni a otros paquetes; conservar únicamente Microsoft.Data.SqlClient. La compilación correcta no significa ausencia de avisos de seguridad. La versión 8.0.2 de Pkcs existe en NuGet, pero el usuario decidió no añadirla como referencia directa. Los avisos siguen sin resolver; una actualización futura de SqlClient deberá auditarse antes de afirmar corrección.

Se actualizaron install_script y start_skill del borrador de entorno para restaurar, compilar y ejecutar las pruebas existentes, sin declarar nuevos secretos ni cambiar dominios. Guardado confirmado; la publicación y restauración en otra tarea no se han validado.

### Decisión posterior a la entrega de etapa 2

El usuario eligió «Mantener únicamente Microsoft.Data.SqlClient». No agregar paquetes directos adicionales para fijar dependencias transitivas. Se conservan los avisos de Pkcs documentados, sin desactivar la auditoría y sin afirmar que se resolvieron. La respuesta sobre dependencias no autorizó por sí sola la etapa 3; el usuario la autorizó después con «continúa».

## Etapa 3 — Entrega del 22 de octubre de 2026

- Agregados Frontend/GestionParqueo.UI/GestionParqueo.UI.csproj (Windows Forms net8.0-windows, sin paquete directo adicional), Program.cs, FrmPrincipal.cs y Controles/UcLogin.cs; reservada Forms/.gitkeep. La solución y el lock de NuGet incluyen la UI.
- FrmPrincipal usa MenuStrip arriba y Panel Dock Fill; CargarControl(UserControl) reemplaza y desecha el control anterior. Al inicio y al cerrar sesión presenta UcLogin y oculta el menú. Tras acceso válido, muestra saludo y submenús según Administrador/Cajero. Las opciones de módulos posteriores están visibles según rol pero deshabilitadas hasta implementar sus pantallas.
- UcLogin usa controles estándar, campos Usuario/Contraseña, contraseña oculta, Enter para ingresar, validación de campos, Sesion.Iniciar y un evento AccesoConcedido. Captura SqlException para explicar un fallo de conexión. No replica hash ni consulta SQL en el frontend.
- Restauración --use-lock-file y compilación Release de toda la solución: cero errores y cero advertencias. No se ejecutó la UI porque la máquina es Linux. Inicio, login SQL, menú y cierre de sesión deben comprobarse en Windows con LocalDB/SQL Server y autenticación de Windows.
- README se amplió con rutas, pasos Visual Studio, prueba manual, limitaciones, problemas y resumen de POO. La guía rápida ahora indica cómo iniciar GestionParqueo.UI.
- El destino Git de este proyecto es la rama main de AlejandrouUmgGt/Gesti-nParqueoSolution. La captura mostrada por el usuario correspondía a otro repositorio, GestorDePedidosSolution. Conservar este destino para las entregas de parqueo.
