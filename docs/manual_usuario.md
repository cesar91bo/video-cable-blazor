# Manual de usuario de Video Cable
Gestión administrativa de clientes servicios facturación y cobranza

Versión 1.0 | 12 de octubre de 2026

Este manual explica cómo realizar las tareas del sistema Video Cable. Está dirigido a la persona que atiende clientes, registra servicios, genera facturas, cobra y consulta información. Las instrucciones utilizan los nombres de los botones y campos de las pantallas.

Para una tarea puntual, buscá el capítulo en el índice y seguí los pasos en orden. Al guardar, verificá el mensaje y el resultado en el listado. Si aparece un error, leé la explicación del capítulo de problemas frecuentes antes de repetir la operación.

## Qué hace el sistema
Administra clientes y planes, instalaciones y suscripciones, cajas de distribución, reclamos técnicos, facturación mensual e individual, autorización de comprobantes en ARCA, cobranza, caja diaria, estados de cuenta, presupuestos y reportes.

El registro de un cliente no crea por sí solo su suscripción. Generar facturas del mes crea cargos internos; la autorización fiscal se hace desde la columna ARCA. Cobrar registra un pago y reduce la deuda; el comprobante de pago documenta ese cobro.

## Índice
2 — Guía rápida e Inicio
3 — Clientes
4 — Servicios y cajas de distribución
5 — Suscripciones
6 — Facturación mensual
7 — Facturación electrónica ARCA
8 — Facturas manuales e impresión
9 — Caja diaria
10 — Cobranza y estado de cuenta
11 — Reclamos técnicos
12 — Presupuestos y reportes de ventas y cobros
13 — Reporte de deudas y Configuración
14 — Problemas frecuentes
15 — Práctica de capacitación
16 — Constancia de entrega y capacitación

Este documento describe la versión indicada. Si cambia una pantalla o un procedimiento, corresponde actualizar el manual.

# Guía rápida e Inicio
## Antes de empezar
1. Abrí el sistema desde el acceso o la dirección que te entregó el responsable de la instalación.
2. Si el menú está contraído, pulsá el botón de tres líneas para desplegarlo.
3. En Inicio, pulsá Actualizar para consultar el resumen operativo.
4. Revisá si la caja diaria está abierta antes de registrar pagos.

Inicio muestra el estado de caja, lo cobrado hoy, lo facturado en el mes y la situación general de los clientes. Normal identifica activos sin deuda; Precaución, activos con un mes adeudado; Deudores, activos con dos o más. Otros reúne situaciones como suspensión, baja o ausencia de una suscripción activa. El Reporte de deudas tiene criterios específicos por servicio, explicados en el capítulo 13.

## Para dar de alta un servicio a un cliente
1. Buscá al cliente en Clientes. Si no existe, registralo.
2. Comprobá que el plan y la caja de distribución estén cargados.
3. Entrá en Suscripciones y creá la instalación con cliente, servicio, caja, dirección y precio final.
4. Verificá que el estado sea Activo si debe entrar en la generación mensual.

## Para facturar el mes
1. Revisá las suscripciones y sus precios.
2. Entrá en Facturación mensual, elegí mes y año y pulsá Generar facturas.
3. Revisá el listado. Para una factura fiscal, seguí Preparar factura fiscal y después Autorizar / recuperar ARCA.
4. Usá Ver / imprimir para entregar el comprobante correspondiente.

## Para cobrar
1. Abrí Caja diaria si no hay una caja abierta.
2. En Cobranza, buscá y seleccioná al cliente y consultá su deuda.
3. Cargá el monto recibido y el medio de pago y pulsá Registrar pago.
4. Comprobá el saldo actualizado y entregá el comprobante de pago.

## Para cerrar la jornada
Revisá cobros y movimientos, compará los importes con los medios recibidos y cerrá la caja con el monto final informado. Consultá Reportes para controlar lo cobrado y Reporte de deudas para organizar el seguimiento de saldos.

# Clientes
La ficha del cliente reúne sus datos personales, de contacto y fiscales. La dirección de esta ficha puede diferir de la dirección donde está instalado el servicio; esa instalación se registra en Suscripciones.

## Buscar antes de crear
1. Entrá en Clientes.
2. Escribí nombre, código, documento o CUIT en Buscar.
3. Pulsá Buscar o Enter y revisá las coincidencias.
4. Si ya existe, usá Editar en su fila. No crees otra ficha para corregir un dato.

El listado muestra hasta 200 resultados. Si no encontrás una persona, escribí un dato más específico; que no esté en la primera lista no significa que no exista.

## Registrar un cliente
1. Pulsá Nuevo cliente.
2. Revisá el código propuesto y completá Nombre.
3. Elegí Tipo doc. y completá Nro. documento cuando corresponda. Si no tiene identificación, seleccioná Sin documento y dejá el número vacío.
4. Completá CUIT cuando corresponda, con 11 números sin guiones.
5. Elegí la Condición IVA real del cliente.
6. Completá Dirección principal / contacto y, si los tenés, localidad, teléfono, email y observaciones.
7. Dejá Activo marcado para un cliente en operación y pulsá Guardar.
8. Verificá el mensaje de guardado y la ficha en el listado.

## Corregir o desactivar
Buscá al cliente, pulsá Editar, modificá los datos y guardá. Cancelar descarta la edición del formulario. Para desactivar una ficha, desmarcá Activo y guardá; revisá además sus suscripciones si se debe detener la facturación del servicio.

## Controles de datos
El sistema controla códigos repetidos, CUIT repetidos y coincidencias de tipo y número de documento. Nombre y dirección son obligatorios; el email, si se informa, debe tener un formato válido.

Para una factura A se necesita un CUIT válido y la condición IVA correcta. Una factura B a consumidor final puede emitirse sin documento dentro del importe permitido, explicado en el capítulo 7. No cargues documentos inventados para superar una validación.

# Servicios y cajas de distribución
## Crear o modificar un plan
1. Entrá en Servicios / Planes y pulsá Nuevo servicio.
2. Completá código y descripción que permitan reconocer el plan.
3. Elegí Tipo servicio, por ejemplo cable, fibra, internet o combo.
4. Elegí Tipo cobro. Los planes de cobro mensual son los utilizados en Suscripciones.
5. Ingresá Precio actual como precio final que paga el cliente, con IVA incluido.
6. Marcá Activo si el plan debe estar disponible y pulsá Guardar.

Para cambiar un plan existente, pulsá Editar en su fila. El precio del plan se propone al elegirlo en una suscripción. La facturación mensual toma el Precio actual guardado en la suscripción: cambiar el catálogo de planes no actualiza automáticamente todas las suscripciones ni las facturas ya generadas.

Cuando haya un aumento, revisá y guardá el precio de las suscripciones que corresponda modificar antes de generar el próximo período.

## Registrar una caja de distribución
Las cajas de distribución son puntos de la red donde se conectan servicios. Caja diaria, en cambio, registra movimientos de dinero.

1. Entrá en Cajas de distribución y pulsá Nueva caja.
2. Completá Código y Descripción.
3. Indicá Dirección / Referencia y Zona para localizarla.
4. Si conocés las coordenadas, completá Latitud y Longitud. Revisá que correspondan a la ubicación real.
5. Marcá Activa y pulsá Guardar.

Usá Editar para corregir una caja o cambiar su disponibilidad. Código y descripción son obligatorios.

## Consultar la red
La pantalla permite actualizar el mapa y el resumen de cajas. Desde el resumen, usá la acción de ver clientes para consultar los asociados a una caja. El mapa utiliza las coordenadas registradas; una caja sin coordenadas necesita esos datos para ubicarse correctamente.

## Comprobación final
Antes de cargar una instalación, verificá que el servicio mensual y la caja estén disponibles. Usá nombres claros para evitar seleccionar un plan o una caja similares por error.

# Suscripciones
Una suscripción une un cliente con un servicio y una caja de distribución. Guarda la instalación, el precio final y el estado que determina si se genera el cargo mensual.

## Dar de alta una instalación
1. Entrá en Suscripciones y pulsá Nueva suscripción.
2. Buscá al cliente y seleccionalo en la lista de resultados.
3. Elegí Servicio mensual. Revisá el Precio actual propuesto.
4. Buscá y seleccioná la caja de distribución.
5. Completá Número de precinto si corresponde.
6. Indicá Fecha alta y Estado.
7. Completá Dirección de instalación. Agregá localidad, referencia y observaciones útiles para ubicar el servicio.
8. Pulsá Guardar y verificá la nueva fila.

La dirección de instalación es obligatoria. El precio admite valores desde cero, aunque para emitir una factura fiscal los importes deben ser positivos. El precinto admite solamente números, hasta 30 dígitos.

## Elegir el estado
- Activo: la suscripción participa en la generación de facturas mensuales.
- Suspendido: no participa mientras permanezca en ese estado.
- Baja: no participa y requiere Fecha baja.

La fecha de baja no puede ser anterior a la de alta. Cambiar el estado no elimina las facturas ni los pagos anteriores.

## Modificar precio dirección o servicio
1. Pulsá Editar en la suscripción.
2. Modificá el dato necesario y revisá el resto del formulario.
3. Si elegís otro servicio, revisá nuevamente el precio, porque se propone el del plan seleccionado.
4. Guardá y comprobá el resultado.

## Duplicados y precintos
El sistema impide dos suscripciones activas del mismo servicio para el mismo cliente. Un precinto no puede pertenecer a otro cliente en la misma caja mientras la suscripción que lo usa no esté de baja. Ante un conflicto, verificá cliente, caja y número; no cambies datos al azar.

Para detener cargos futuros por una baja, actualizá la suscripción. Desactivar solamente al cliente no reemplaza este paso.

# Facturación mensual
Esta pantalla genera cargos para las suscripciones activas y permite consultar el período, preparar facturas fiscales, autorizarlas e imprimirlas.

## Generar los cargos del mes
1. Antes de empezar, revisá servicios activos, precios y estados de suscripción.
2. Entrá en Facturación mensual.
3. Seleccioná Mes y Año del período que vas a cobrar.
4. Pulsá Generar facturas.
5. Revisá Facturas generadas, Omitidas por duplicado y Suscripciones evaluadas.
6. Consultá el listado y verificá cliente, servicio, caja, período e importe.

La generación usa el precio guardado en cada suscripción. Si existe una factura para esa suscripción y período, la omite. Repetir el proceso no es una forma de cambiar el importe de una factura existente.

## Qué se crea en este paso
Se crea un comprobante interno X con estado No fiscal y cobranza Pendiente. Generar facturas no solicita un CAE en ARCA. Para convertir un cargo a factura fiscal, usá el procedimiento del capítulo 7.

Consultar período carga las facturas existentes sin generar cargos nuevos. Si la columna ARCA está fuera de vista, desplazá horizontalmente la tabla hacia la derecha.

## Interpretar las columnas
- Estado cobranza: Pendiente indica saldo sin pagar; Parcial indica un pago incompleto; Pagada indica saldo cancelado.
- Estado fiscal: No fiscal corresponde al cargo interno; Pendiente ARCA indica que falta autorización; un CAE identifica una autorización fiscal y muestra el ambiente.
- Comprobante: muestra la numeración fiscal cuando está disponible.
- Total: es el importe final del cargo.
- Ver / imprimir: abre el detalle y las opciones de impresión.

Una factura puede estar Pagada y continuar Pendiente ARCA. Haber cobrado el dinero no autoriza automáticamente el comprobante fiscal.

## Antes de entregar una factura fiscal
Comprobá que tenga CAE de Producción. Un comprobante X o una autorización de Homologación no reemplaza una factura fiscal real. Verificá también que la misma operación no se haya facturado previamente en el sistema anterior.

# Facturación electrónica ARCA
## Preparar y autorizar una factura
1. En Facturación mensual, ubicá la fila correcta y verificá cliente, total y período.
2. Si figura No fiscal, pulsá Preparar factura fiscal.
3. Leé el resultado. Debe indicar el tipo de factura y Produccion cuando se requiere emisión real.
4. Si está pendiente y necesita actualizar sus datos o ambiente, pulsá Actualizar datos fiscales y ambiente. El sistema impide modificar un intento enviado cuyo estado todavía deba recuperarse.
5. Pulsá Autorizar / recuperar ARCA una sola vez y esperá el resultado.
6. Verificá el CAE, el ambiente Producción y el número de comprobante. Después abrí Ver / imprimir.

El botón de autorización envía el comprobante sin una confirmación adicional. Usalo para una operación real pendiente de facturar. El tipo A, B o C se determina según la condición IVA del emisor y del receptor; no se elige cambiando el documento al azar.

## Documento e IVA
Para un emisor responsable inscripto, los receptores responsables inscriptos o monotributistas se preparan como factura A y necesitan CUIT válido. La factura B a consumidor final puede prepararse sin documento: el sistema informa receptor sin identificar a ARCA.

Desde $10.000.000 se exige identificación del consumidor final. Si se informa un documento, el sistema valida sus datos. El importe corresponde a la regla vigente indicada por ARCA al elaborar este manual.

El precio es final con IVA incluido. Con total $27.000 e IVA del 21 %, el desglose es neto $22.314,05 e IVA $4.685,95. El total continúa siendo $27.000.

## Si la respuesta no llega
Usá Autorizar / recuperar ARCA en la misma fila para consultar y recuperar el resultado. No crees otra factura para sustituir una emisión de resultado incierto. Si hay una emisión pendiente de recuperación, resolvela antes de autorizar otra.

Una factura autorizada conserva sus datos fiscales. La pantalla no ofrece un procedimiento de nota de crédito; ante una corrección de una factura autorizada, consultá al responsable antes de actuar.

Referencia de identificación del receptor: https://www.arca.gob.ar/fe/emision-autorizacion/datos-comprobantes.asp

# Facturas manuales e impresión
## Cargar un cargo de un período anterior
La carga manual permite registrar deuda durante la migración o cargar un período puntual. Requiere un cliente y una suscripción existentes.

1. Entrá en Facturación mensual y pulsá Cargar factura manual.
2. Buscá y seleccioná el cliente.
3. Elegí la suscripción de la instalación correspondiente.
4. Indicá Mes y Año del cargo.
5. Revisá Servicio facturado. Puede ser distinto del servicio actual si el cargo corresponde a otro plan anterior.
6. Ingresá Monto como importe final.
7. Completá Fecha emisión y Fecha vencimiento y agregá una observación, por ejemplo el motivo de la carga.
8. Pulsá Guardar factura manual y revisá el período que se muestra después del guardado.

No uses esta función para volver a emitir en ARCA una operación que ya tiene una factura real del sistema anterior. La carga del saldo interno y una nueva emisión fiscal tienen consecuencias diferentes.

## Imprimir una factura
1. En el listado, pulsá Ver / imprimir en la fila correspondiente.
2. Revisá cliente, servicio, período, total y datos del comprobante.
3. Pulsá Imprimir / Guardar PDF.
4. En el diálogo del navegador, elegí la impresora o Guardar como PDF y confirmá.

Un comprobante X puede imprimirse como interno. Para una factura A, B o C, la impresión se habilita cuando está autorizada. Revisá que la autorización sea de Producción si se entrega como factura fiscal real.

## Imprimir un comprobante de pago
Después de registrar un cobro, la opción de imprimir abre el comprobante de pago. Revisá fecha, cliente, importe y medio utilizado antes de imprimir. Este documento acredita el pago registrado; la factura fiscal se obtiene desde Facturación mensual.

## Control al entregar
Entregá el documento adecuado para la tarea. Guardá el PDF con un nombre que identifique cliente, fecha y número de comprobante. Una reimpresión de un comprobante existente no requiere crear otra factura ni registrar otro pago.

# Caja diaria
Caja diaria registra la apertura, cobros, movimientos manuales y cierre. Para registrar un pago en Cobranza tiene que haber una caja abierta.

## Abrir la caja
1. Entrá en Caja diaria.
2. Si no hay caja abierta, completá Monto inicial con el importe de apertura. Puede ser cero, pero no negativo.
3. Agregá Observaciones si necesitás identificar la apertura.
4. Pulsá Abrir caja y comprobá el estado Abierta.

El sistema admite una caja abierta. Si el monto inicial es mayor que cero, genera el movimiento de apertura; no lo cargues nuevamente como ingreso manual.

## Registrar un movimiento manual
1. Elegí Tipo: Ingreso, Egreso o Ajuste.
2. Ingresá Monto positivo y un Concepto que explique la operación.
3. Pulsá Registrar y verificá la fila en Movimientos de caja.

Ingreso aumenta el saldo; Egreso lo reduce. Ajuste se suma al saldo en esta versión. Para una salida de dinero, usá Egreso. Los cobros de clientes se registran desde Cobranza y crean su movimiento automáticamente; no los dupliques como ingresos manuales.

## Revisar y cerrar
1. Revisá los movimientos y el Monto sistema.
2. Compará el total con los cobros y medios recibidos durante la jornada.
3. Completá Monto final informado.
4. Registrá Observaciones cierre cuando exista una diferencia o información que aclarar.
5. Pulsá Cerrar caja y leé el resultado.

La diferencia es Monto final informado menos Monto sistema. Por ejemplo, si el sistema indica $50.000 y se informan $49.000, la diferencia es -$1.000.

El monto del sistema suma ingresos, cobros y ajustes no anulados y resta egresos. Incluye cobros registrados con distintos medios; no debe interpretarse automáticamente como efectivo físico. Conciliá esos medios en Reportes antes de informar el cierre.

Al cerrar, la caja deja de estar disponible para nuevos cobros. La pantalla no tiene un botón de reapertura de la misma caja; si necesitás una corrección, consultá al responsable.

# Cobranza y estado de cuenta
## Registrar un pago total o parcial
1. Comprobá que exista una Caja diaria abierta.
2. Entrá en Cobranza, buscá al cliente y seleccionalo en la lista.
3. Pulsá Ver deuda y revisá las facturas pendientes y parciales.
4. En Monto cobrado, cargá exactamente lo recibido. Cobrar total propone la deuda completa.
5. Elegí Medio de pago y completá Referencia cuando corresponda, por ejemplo el identificador de una transferencia.
6. Agregá Observaciones si necesitás explicar el pago.
7. Marcá la opción de imprimir el comprobante si querés entregarlo al registrar.
8. Pulsá Registrar pago y esperá el mensaje de éxito.
9. Revisá el nuevo saldo y el comprobante.

El monto tiene que ser mayor que cero y no puede superar la deuda total. Esta pantalla registra un medio de pago por operación. Si necesitás dividir un cobro entre medios, registrá cada importe por separado y revisá el saldo después de cada pago.

El pago se aplica automáticamente a las facturas de los períodos más antiguos primero. Si alcanza para cancelar una factura, queda Pagada; si solamente cancela una parte, queda Parcial. El movimiento de caja se genera automáticamente.

Ejemplo: deuda total $27.000 y pago $10.000. El saldo restante es $17.000. Para completar el cobro, registrá después el importe restante; no vuelvas a registrar $27.000.

## Consultar e imprimir el estado de cuenta
1. Entrá en Estado de cuenta.
2. Buscá y seleccioná el cliente.
3. Pulsá Ver estado de cuenta y revisá período, servicio, vencimiento, importe, pagado y saldo.
4. Usá Imprimir para entregar el detalle o guardar un PDF desde el navegador.
5. Si tiene deuda, usá Ir a cobranza para continuar con el cobro.

El estado de cuenta muestra saldos pendientes y parciales. Un listado vacío puede significar que no hay deuda registrada; verificá que seleccionaste al cliente correcto y que sus cargos ya fueron generados.

# Reclamos técnicos
Reclamos permite registrar el problema informado por un cliente, asociarlo a su instalación y seguir el trabajo hasta su resolución.

## Registrar un reclamo
1. Entrá en Reclamos y pulsá Nuevo reclamo.
2. Buscá y seleccioná al cliente.
3. Seleccioná Suscripción y Caja cuando correspondan al problema.
4. Revisá Fecha y elegí Tipo y Prioridad.
5. Indicá el Estado inicial.
6. En Descripción del reclamo, explicá qué ocurre, desde cuándo y en qué instalación. Evitá descripciones como “no anda” sin más detalle.
7. Usá Observaciones internas para datos de trabajo y Resolución para lo que se hizo al solucionarlo.
8. Pulsá Guardar y verificá la fila.

Cliente y descripción son obligatorios. Si un cliente tiene varias instalaciones, revisá que el reclamo quede asociado a la correcta.

## Dar seguimiento
El listado ofrece Editar e Imprimir y, según el estado, las acciones En proceso, Resolver y Cerrar. Usá Editar para completar la descripción o la resolución. Registrá el trabajo realizado para que otra persona pueda entender cómo se resolvió.

En proceso indica atención en curso. Resuelto identifica un problema solucionado. Cerrado finaliza el seguimiento administrativo. Un cambio de estado no registra un cobro ni emite una factura.

## Buscar e imprimir
1. Filtrá por cliente o texto, Estado, Tipo, Prioridad y fechas Desde y Hasta.
2. Aplicá el filtro con el botón de búsqueda. Restablecer limpia los filtros.
3. Usá Imprimir en una fila para la orden individual.
4. Para el conjunto filtrado, usá Imprimir listado.

El listado impreso requiere un período de hasta 31 días y un máximo de 100 reclamos. Si no se habilita la impresión o aparece un mensaje, reducí fechas o aplicá más filtros. Desde no puede ser posterior a Hasta.

## Qué comprobar al terminar
La descripción tiene que identificar el problema; la instalación y caja deben corresponder al cliente; la resolución debe explicar el resultado. Esto facilita la continuidad del trabajo y evita abrir otro reclamo por falta de información del anterior.

# Presupuestos y reportes de ventas y cobros
## Preparar un presupuesto
1. Entrá en Presupuestos y completá el número que corresponda.
2. Buscá un cliente existente o completá los datos del destinatario en el formulario.
3. Revisá dirección, teléfono, validez y observaciones.
4. Elegí un Servicio / concepto, cargá Cantidad y Precio y pulsá Agregar.
5. Para un concepto libre, activá la opción de concepto manual y completá descripción, cantidad y precio unitario.
6. Revisá los ítems y el total. Usá Quitar en un ítem si lo cargaste por error.
7. Pulsá Imprimir / Guardar PDF y elegí el destino en el navegador.

Se requiere al menos un ítem y el nombre del destinatario para imprimir. La cantidad debe ser positiva y el precio no puede ser negativo. Limpiar inicia otro presupuesto y descarta los datos del formulario actual.

El presupuesto es informativo y no reemplaza una factura. Esta pantalla arma el documento para imprimir; no ofrece un listado de presupuestos guardados para recuperar después. Conservá el PDF si necesitás consultarlo nuevamente.

## Consultar ventas o cobros
1. Entrá en Reportes.
2. En Reporte, elegí Cobrado o Facturado.
3. Indicá Desde y Hasta inclusive, o usá Hoy, Ayer, Esta semana, Este mes o Mes anterior.
4. Elegí Agrupar por: día, semana, mes, cliente o servicio. En Cobrado también podés agrupar por medio de pago.
5. Si querés limitar a un cliente, completá nombre o código; dejá vacío para todos.
6. Pulsá Consultar. Si cambiás fechas o cliente, consultá nuevamente antes de imprimir.
7. Revisá total, operaciones y clientes y pulsá Imprimir reporte.

Cobrado usa la fecha del pago y excluye pagos anulados o eliminados. Facturado usa la fecha de emisión e incluye comprobantes pendientes de autorización; excluye anulados, rechazados o eliminados. Por eso Facturado no equivale a total de facturas autorizadas por ARCA.

Al agrupar por servicio o medio de pago, una operación puede figurar en varios grupos. Usá el total general para comparar el conjunto. Las semanas se muestran de lunes a domingo dentro del período seleccionado.

# Reporte de deudas y Configuración
## Consultar clientes con saldo
1. Entrá en Reporte de deudas.
2. Buscá por nombre, código, documento o CUIT si querés un cliente concreto.
3. Elegí Orden, Estado deuda y Servicio.
4. Pulsá Buscar o Actualizar y revisá el saldo y los períodos de cada cliente.
5. Usá las acciones de la fila para consultar su cuenta o continuar con el cobro.
6. Pulsá Imprimir reporte para trabajar con el listado filtrado.

Los estados orientan el seguimiento según períodos adeudados por servicio. Para cable, Atención corresponde a dos meses y Riesgo desde tres. Para internet, fibra o combo, Atención corresponde a un mes y Riesgo desde dos.

La etiqueta Riesgo / Suspender no corta automáticamente una conexión ni cambia la suscripción. Si se decide suspender, registrá el cambio de estado en Suscripciones según el procedimiento de la empresa.

## Guardar datos de la empresa
En Configuración se cargan razón social, nombre fantasía, CUIT, condición IVA, dirección y datos de contacto. Modificá los datos que corresponda, pulsá Guardar cambios y verificá el mensaje de éxito. Cancelar vuelve a cargar lo guardado.

## Seleccionar el ambiente de emisión
1. En Configuración, buscá Facturación electrónica ARCA.
2. En Ambiente de emisión, elegí Homologación para pruebas o Producción para comprobantes reales.
3. Si elegís Producción, marcá la confirmación de emisión real.
4. Pulsá Guardar cambios y comprobá el resultado.

IVA de los servicios indica la tasa incluida en los precios finales de las facturas A y B. El límite mensual es un aviso: vacío o cero lo desactiva. Se advierte desde el 80 % o cuando la próxima factura alcanza o supera el importe; permite continuar. Considera A, B y C autorizadas con CAE por fecha de emisión, separadas por ambiente.

## Certificado punto de venta y prueba
Desde Configuración abrí Certificados, punto de venta y prueba de conexión. Elegí el ambiente a configurar, cargá el PFX/P12 y su contraseña, guardá el certificado y el punto de venta y pulsá Probar conexión sin emitir.

Ese selector configura o prueba un ambiente; el ambiente de emisión se guarda en Configuración. La prueba de conexión no emite una factura. La gestión de certificados y contraseñas corresponde al responsable designado; este manual no contiene claves de acceso.

# Problemas frecuentes
## No encuentro al cliente
Usá la búsqueda por nombre, código, documento o CUIT. En Clientes se muestran hasta 200 resultados; afiná la búsqueda. En un formulario que ofrece búsqueda y lista, seleccioná la persona en la lista además de escribir el nombre.

## No se generó una factura del mes
Revisá mes y año, estado Activo de la suscripción y si ya existe un cargo para ese período. Revisá también que el cliente tenga una suscripción; importar la ficha del cliente no crea automáticamente sus servicios.

## El importe no es el esperado
La generación toma el precio de la suscripción. Revisalo en Suscripciones. Cambiar el precio del plan no cambia todas las suscripciones ni una factura ya generada. Si el cargo ya existe, consultá al responsable antes de reemplazarlo.

## La factura quedó preparada para Homologación
Guardá Producción como Ambiente de emisión en Configuración. En la factura pendiente, pulsá Actualizar datos fiscales y ambiente y verificá que el resultado indique Produccion. El sistema no permite cambiar un intento enviado que todavía necesita recuperar su estado.

## Error de ambiente o de documento al autorizar
Revisá la configuración guardada y el ambiente de la factura. Para una A, verificá CUIT y condición IVA del cliente. Para consumidor final sin documento, dejá vacío el número en su ficha y revisá que el total permita la emisión sin identificación. Después actualizá los datos fiscales de la factura pendiente.

## ARCA no responde o hay una emisión pendiente
Volvé a usar Autorizar / recuperar ARCA en la misma fila. No dupliques la operación ni cambies una numeración manualmente. Si continúa, enviá al soporte el mensaje completo y la identificación de la factura.

## No puedo registrar el pago
Revisá que la caja esté abierta, el cliente seleccionado y la deuda cargada. El importe debe ser positivo y no superar el saldo. Si hubo un error después de pulsar Registrar pago, consultá saldo y pagos recientes antes de volver a pulsarlo.

## No puedo imprimir o el reporte está vacío
Para una factura fiscal se necesita autorización. En Reportes, pulsá Consultar después de cambiar filtros. Para reclamos, reducí a 31 días y hasta 100 filas. Revisá la vista previa y seleccioná impresora o Guardar como PDF.

## Cómo pedir ayuda
Indicá pantalla, cliente o código, período, acción realizada, mensaje completo y resultado esperado. Adjuntá una captura con esos datos. No incluyas contraseñas ni la clave fiscal.

# Práctica de capacitación
Esta hoja permite verificar tareas concretas. El responsable completa la fecha y el resultado después de la práctica. Usá operaciones reales pendientes o un entorno de Homologación preparado para ejercicios; no emitas facturas reales ficticias ni registres cobros inexistentes.

## Ejercicios
1. Buscar un cliente existente, identificar su código y abrir su ficha sin crear un duplicado.
2. Registrar o corregir datos de contacto y comprobar que quedaron guardados.
3. Identificar el plan, la caja, la dirección, el precinto, el estado y el precio final de una suscripción.
4. Consultar un período de facturación y distinguir No fiscal, Pendiente ARCA y CAE, así como Pendiente, Parcial y Pagada.
5. Explicar la diferencia entre Generar facturas, Autorizar / recuperar ARCA y Registrar pago.
6. Revisar el ambiente de emisión guardado y distinguirlo del ambiente elegido para una prueba de conexión.
7. Realizar el procedimiento de una factura fiscal con supervisión en el entorno y operación definidos por el responsable.
8. Registrar un pago total o parcial que realmente corresponda y comprobar su aplicación y comprobante.
9. Consultar e imprimir el estado de cuenta de un cliente.
10. Registrar o actualizar un reclamo y dejar documentada su resolución.
11. Crear un presupuesto con un ítem y guardarlo como PDF.
12. Consultar Reportes por fechas y medio de pago y revisar el Reporte de deudas.
13. Revisar movimientos de caja, explicar una diferencia y completar un cierre cuando corresponda.
14. Identificar qué información enviar al soporte ante un error.

## Registro de la práctica
Fecha de práctica __________________________
Usuario participante __________________________
Responsable que acompaña __________________________
Entorno y operación utilizados __________________________
Ejercicios realizados sin ayuda __________________________
Ejercicios realizados con ayuda __________________________
Temas que requieren repaso __________________________
Fecha prevista para el repaso __________________________

Resultado observado y comentarios
____________________________________________________________________
____________________________________________________________________
____________________________________________________________________

# Constancia de entrega y capacitación
Sistema Video Cable | Manual de usuario versión 1.0 | 12 de octubre de 2026

Esta constancia registra la entrega del material y, cuando efectivamente se realice, la capacitación y práctica. Completá cada casillero según lo ocurrido. La recepción del manual por sí sola no certifica que todos los ejercicios se realizaron o que todas las dudas se resolvieron.

## Entrega del material
Fecha y hora __________________________
Empresa __________________________
Nombre del usuario __________________________
Responsable de la entrega __________________________
Medio de entrega del manual __________________________
Versión o fecha de la instalación utilizada __________________________

Material entregado
[ ] Manual de usuario   [ ] Acceso al sistema   [ ] Datos de contacto del soporte
Ubicación del manual o copia entregada __________________________

## Actividad realizada
[ ] Entrega del manual solamente
[ ] Explicación de las pantallas y tareas habituales
[ ] Demostración por el responsable
[ ] Práctica por el usuario
[ ] Repaso o capacitación adicional

Fecha duración y modalidad de la capacitación
____________________________________________________________________

Temas explicados y ejercicios realizados
____________________________________________________________________
____________________________________________________________________

Dudas planteadas y respuestas o acciones pendientes
____________________________________________________________________
____________________________________________________________________

Contacto y canal acordados para consultas __________________________

## Firmas
Usuario que recibe __________________________    Fecha ______________
Responsable de entrega o capacitación __________________________
Responsable de la empresa si participa __________________________

Las firmas dejan constancia de lo indicado en esta hoja. Conservá una copia del manual entregado y de esta constancia completada.
