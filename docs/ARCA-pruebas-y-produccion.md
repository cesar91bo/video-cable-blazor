# Facturación ARCA: pruebas y puesta en producción

El flujo implementado emite facturas A, B y C por servicios, en pesos, mediante WSFEv1 y CAE. La letra se determina según la condición IVA de la empresa y el cliente. Los precios de los servicios se consideran finales, IVA incluido. Las facturas X continúan siendo internas.

## Preparar el sistema

1. Hacer una copia de seguridad de la base de datos.
2. Iniciar la versión actualizada. El arranque aplica las migraciones. La nueva migración agrega ambiente, datos fiscales del receptor y reserva de emisión. Los comprobantes fiscales anteriores conservan el ambiente de su punto de venta; los que no tenían uno se consideran de homologación, que era el ambiente fijo del flujo anterior.
3. En **Configuración**, revisar CUIT, razón social, domicilio y condición IVA de la empresa. Elegir **Homologación** y la alícuota de los servicios (el valor inicial es 21 %). Guardar los cambios. Confirmar con el responsable contable que esos datos corresponden a la empresa.
4. En **Clientes**, cargar una condición IVA válida y DNI, CUIT o CUIL. Por decisión del sistema, toda factura fiscal exige identificar al cliente; no se usa un umbral monetario legal fijo que podría quedar desactualizado. Para clientes inscriptos, exentos o monotributistas se exige CUIT.

## Obtener el certificado de homologación

El certificado de producción no reemplaza al de pruebas. Solicitar en ARCA el acceso a WSASS, generar un pedido CSR y gestionar el certificado de homologación. Asociar el certificado al servicio **wsfe** para el CUIT que emitirá. Utilizar los valores que ARCA permita en ese ambiente.

El sistema recibe un archivo **PFX/P12 con clave privada** y su contraseña. Un archivo `.crt` o `.cer` por sí solo no alcanza. Si se dispone de la clave privada y del certificado descargado, se puede construir el PFX con OpenSSL:

```powershell
openssl pkcs12 -export -out homologacion.pfx -inkey clave.key -in certificado.crt
```

OpenSSL pedirá la contraseña de exportación; esa es la contraseña que se carga en el formulario. No enviar al chat claves privadas ni contraseñas.

Fuentes oficiales: [certificados y WSASS](https://www.afip.gob.ar/ws/documentacion/certificados.asp), [autenticación WSAA](https://www.afip.gob.ar/ws/documentacion/wsaa.asp).

## Probar sin emitir

1. Entrar a **Configuración → Certificados, punto de venta y prueba de conexión**.
2. Seleccionar **Homologación**, elegir el `.pfx`/`.p12`, escribir su contraseña y pulsar **Guardar certificado**.
3. Cargar el número del punto de venta habilitado en ARCA para web service y CAE. Guardarlo en el sistema no lo da de alta en ARCA.
4. Pulsar **Probar conexión sin emitir**. Verifica apertura del certificado, WSAA y habilitación del punto de venta. No solicita un CAE.

La renovación del token es automática. Se reutiliza mientras conserva más de cinco minutos de vigencia; cuando vence se genera uno nuevo. Un certificado vencido, sin clave privada o de otro ambiente produce un error, sin emitir.

## Probar una factura en homologación

1. Usar una base separada para pruebas; no reutilizar registros de facturas de prueba para ventas reales.
2. Crear una factura X en **Facturación mensual** para un cliente con sus datos fiscales completos.
3. Pulsar **Preparar factura fiscal**. Se elige A/B/C, se desglosa el IVA y se guarda el ambiente. Esta acción todavía no emite.
4. Revisar el total, los datos del receptor y la fecha de emisión. Para servicios se valida el rango de diez días antes/después de la fecha actual. ARCA también valida la fecha respecto del último comprobante del punto de venta.
5. Pulsar **Autorizar / recuperar ARCA**. Debe aparecer el CAE, su vencimiento y la numeración de cinco dígitos de punto de venta más ocho de comprobante.
6. Abrir **Ver / imprimir**. Revisar subtotal, IVA y total. Deben coincidir con el pedido enviado. El QR debe estar presente y el comprobante debe indicar **HOMOLOGACIÓN — SIN VALIDEZ FISCAL**, incluso al imprimir.
7. Repetir la autorización de esa factura: debe indicar que ya está autorizada y mantener el mismo número y CAE. Usar **Verificar datos ARCA** para consultar el comprobante ya emitido y conciliar los importes/QR de registros anteriores. Esta consulta no emite otro comprobante.
8. Probar los casos A/B/C que correspondan a las condiciones fiscales utilizadas, y un rechazo por datos inválidos. Las notas de crédito/débito y otros regímenes no forman parte de este flujo de facturas de servicios.

### Probar el límite mensual

En Configuración, establecer un importe pequeño para pruebas. Se avisa desde el 80 % del acumulado proyectado o si la nueva factura alcanza/supera el límite. **Cancelar** no envía el comprobante; **Continuar emisión ARCA** permite emitir. Cero o vacío desactiva el aviso. Solo cuenta facturas A/B/C autorizadas, por mes de emisión y ambiente. El límite es una alerta interna, no un cálculo de obligaciones tributarias.

### Cortes, recuperación y varios usuarios

Antes de solicitar el CAE se guardan ambiente, tipo, punto de venta, número reservado y contenido del pedido, sin Token/Sign. Un bloqueo de SQL Server coordina las emisiones de usuarios/procesos que usan esta misma base. Conviene dedicar ese punto de venta a este sistema; otro programa no participa de ese bloqueo.

Si aparece un error después del envío, **no crear otra factura**. Volver a **Autorizar / recuperar ARCA en la misma factura**. El sistema consulta `FECompConsultar`; si el comprobante existe y coincide, guarda el CAE sin emitir nuevamente. Si ARCA confirma que no existe y la numeración sigue libre, reenvía el mismo contenido y número. Si la consulta es incierta o los datos no coinciden, se detiene. Mientras hay un intento incierto, se impide emitir nuevas facturas de esa empresa y ambiente.

Un rechazo concluyente permite corregir el cliente/configuración y usar **Actualizar datos fiscales**. No se pueden modificar los datos de un intento incierto ni cambiar el ambiente antes de recuperarlo.

## Pruebas automáticas sin credenciales reales

Desde la raíz del proyecto:

```powershell
dotnet run --project tests/ArcaChecks/ArcaChecks.csproj
dotnet run --project tests/ArcaChecks/ArcaChecks.csproj -- --sql
dotnet run --project tests/ReportesChecks/ReportesChecks.csproj
```

La segunda prueba requiere SQL LocalDB `MSSQLLocalDB`. Crea una base temporal con nombre aleatorio y archivos en TEMP, aplica las migraciones y la elimina al terminar. Usa certificado de prueba generado localmente y respuestas WSAA/WSFE simuladas. No se conecta con ARCA ni usa la base de la aplicación. Comprueba emisiones concurrentes, corte después de autorización, recuperación sin duplicados, límites y renovación automática del token.

## Pasar a producción

1. Completar las pruebas de homologación y validar los datos/IVA con el responsable contable.
2. Gestionar el certificado de **producción** en Administración de Certificados Digitales y la relación del servicio **wsfe** para el CUIT emisor. Dar de alta el punto de venta electrónico correspondiente. [Pasos oficiales](https://www.arca.gob.ar/fe/documentos/AccionesarealizarparaconsumirunWebservicedeFacturaElectr.pdf).
3. Cargar certificado y punto de venta seleccionando **Producción** en Configuración ARCA.
4. Ejecutar **Probar conexión sin emitir** en Producción. Es una comprobación de conexión/habilitación, no una certificación de todas las reglas fiscales ni una emisión de prueba.
5. Usar la base de trabajo real, con su copia de seguridad, sin registros de ensayo. En Configuración seleccionar **Producción**, marcar la confirmación de comprobantes reales y guardar.
6. Preparar y revisar una factura de una operación real. Al autorizar, se solicita confirmación de que es producción. No emitir facturas ficticias para probar en producción.
7. Revisar en ARCA el primer comprobante real y comparar número, receptor, importes, CAE y QR con la impresión del sistema.

## Certificados, claves y copias de seguridad

Los PFX, contraseñas y tokens se protegen con ASP.NET Core Data Protection. Al arrancar se protegen los secretos legados; si el PFX viejo está disponible en su ruta, también se incorpora protegido a la base. Los archivos originales externos no se borran automáticamente: deben guardarse fuera de `wwwroot`, con permisos restringidos.

Las claves persistentes se guardan por defecto en `App_Data/DataProtectionKeys`, fuera de `wwwroot` e ignoradas por Git. En Windows se protegen además con DPAPI para la identidad del proceso. Conservar esa carpeta al actualizar y respaldarla junto con la base. No cambiar la identidad del servicio o trasladar el sistema a otro equipo sin preparar la recuperación de credenciales; DPAPI puede impedir abrir las claves en otra identidad/equipo. Conservar los PFX originales de forma segura para volver a cargarlos si fuera necesario.

Para una instalación permanente se puede configurar una ubicación estable mediante la variable `Arca__DirectorioClaves`. La identidad que ejecuta el sistema debe poder leer/escribir allí. En hosts que no sean Windows se deben restringir los permisos del directorio y configurar protección de las claves en reposo antes de usar producción.

El reloj del servidor debe estar sincronizado y su zona horaria configurada para Argentina; la emisión y la vigencia del token utilizan la hora del servidor.

Documentación utilizada: [Manual WSFEv1](https://www.arca.gob.ar/ws/documentacion/manuales/manual-desarrollador-ARCA-COMPG.pdf), [especificación QR](https://www.arca.gob.ar/fe/qr/documentos/QRespecificaciones.pdf).
