# Plan de reconstrucción - Sistema Video Cable en Blazor

Este documento resume las decisiones tomadas y el plan de trabajo para reconstruir desde cero el sistema actual de Video Cable, reemplazando el WinForms heredado por una aplicación Blazor moderna.

La idea no es copiar el sistema viejo tal cual, sino conservar el negocio que funciona y corregir las deudas técnicas detectadas.

---

## 1. Objetivo general

Crear un nuevo sistema en Blazor para operar el ciclo principal del negocio:

1. Alta y gestión de clientes.
2. Alta y gestión de servicios/planes.
3. Administración de cajas de distribución.
4. Creación y gestión de suscripciones.
5. Generación mensual de facturas internas.
6. Consulta de deuda.
7. Registro de pagos parciales o totales.
8. Caja diaria.
9. Reportes básicos.
10. En una segunda etapa, integración ARCA/AFIP para CAE, certificados y comprobantes fiscales.

---

## 2. División del MVP

### MVP 1A - Operación interna

El MVP 1A no debe depender de ARCA/AFIP. Tiene que permitir operar el negocio internamente aunque todavía no se autoricen comprobantes electrónicos.

Incluye:

- Clientes.
- Servicios/planes.
- Cajas de distribución.
- Suscripciones.
- Estados de suscripción.
- Facturación mensual interna.
- Facturas pendientes, parciales y pagadas.
- Pagos parciales.
- Caja diaria.
- Pantalla de deuda/cobranza rápida.
- Reportes mínimos.

### MVP 1B - Integración fiscal

Se implementa después, una vez que el núcleo del sistema ya funciona.

Incluye:

- Empresa fiscal.
- Punto de venta.
- Certificado digital.
- Token/sign WSAA.
- Solicitud CAE por WSFEv1.
- Factura electrónica.
- QR fiscal.
- PDF fiscal.
- Diagnóstico ARCA/AFIP.

---

## 3. Decisiones principales

### 3.1 Entidad central

La entidad central del nuevo sistema será:

```txt
Suscripcion
```

Una suscripción representa la relación entre:

```txt
Cliente + ServicioPlan + CajaDistribucion + Estado
```

Desde la suscripción salen la facturación mensual, la deuda, los pagos, la suspensión y los reportes.

---

### 3.2 Facturación y cobranza separadas

Regla central:

```txt
Autorizar una factura no significa cobrarla.
```

Por eso:

- `Factura` representa el documento emitido o generado.
- `FacturaElectronica` representa la autorización fiscal, CAE y datos ARCA/AFIP.
- `Pago` representa dinero cobrado.
- `PagoAplicacion` representa qué parte de un pago se aplica a qué factura.
- `MovimientoCaja` representa el impacto en la caja diaria.

---

### 3.3 Período de facturación

No usar string libre tipo `2026-06`.

Usar:

```csharp
public int PeriodoAnio { get; set; }
public int PeriodoMes { get; set; }
```

Regla de unicidad para MVP 1A:

```txt
Una factura por SuscripcionId + PeriodoAnio + PeriodoMes
```

Índice esperado:

```txt
IX_Factura_SuscripcionId_PeriodoAnio_PeriodoMes
```

---

### 3.4 Factura por suscripción

Para el MVP 1A se asume:

```txt
1 factura = 1 suscripción = 1 período
```

Esto simplifica:

- Control de duplicados.
- Deuda.
- Suspensión.
- Cobranza.
- Migración.

Advertencia futura:

Si el cliente pide una sola factura con varios servicios del mismo cliente, se deberá mover la relación con `Suscripcion` desde `Factura` hacia `FacturaItem`.

---

### 3.5 Pagos parciales

El sistema debe soportar pagos parciales desde el inicio.

Ejemplo:

- Cliente debe 3 facturas.
- Paga 2 facturas.
- Queda una pendiente.
- No se cobra recargo en MVP 1A, pero el modelo debe quedar preparado para agregarlo más adelante.

Entidades clave:

```txt
Pago
PagoDetalle
PagoAplicacion
```

---

### 3.6 Recargos

No implementar recargos en MVP 1A.

Pero dejar preparado el modelo para poder agregar después:

- Recargo por mora.
- Cargo de reconexión.
- Intereses.
- Bonificaciones/descuentos.

---

### 3.7 Estados de suscripción

Para MVP 1A usar enum, no tabla.

```csharp
public enum EstadoSuscripcionTipo
{
    Activo = 1,
    Baja = 2,
    Suspendido = 3
}
```

El historial de cambios se guarda en:

```txt
SuscripcionEstadoHistorial
```

---

### 3.8 Medios de pago

Para MVP 1A usar enum.

```csharp
public enum MedioPagoTipo
{
    Efectivo = 1,
    Transferencia = 2,
    Tarjeta = 3,
    Otro = 99
}
```

Más adelante se puede migrar a tabla `MedioPago` si el cliente necesita medios configurables, cuentas bancarias, billeteras, comisiones o conciliación.

---

## 4. Tecnología elegida

### Stack principal

```txt
.NET 8
Blazor Web App
Renderizado interactivo server-side
EF Core
SQL Server
```

### Motivo

Es una aplicación administrativa interna, con operación de oficina, SQL Server y futura integración ARCA/AFIP. Blazor interactivo server-side permite avanzar rápido sin exponer una API pública al principio.

---

## 5. Estructura inicial de solución

Estructura recomendada:

```txt
VideoCable.Blazor/
  VideoCable.Web
  VideoCable.Domain
  VideoCable.Infrastructure
```

### VideoCable.Web

Responsabilidades:

- Componentes Blazor.
- Páginas.
- Layout.
- Formularios.
- Validaciones visuales.
- Servicios de UI.
- Inyección de dependencias.
- Login inicial.

### VideoCable.Domain

Responsabilidades:

- Entidades.
- Enums.
- Reglas simples del dominio.
- Clases base de auditoría.
- Constantes de negocio.

### VideoCable.Infrastructure

Responsabilidades:

- DbContext.
- Configuración EF Core.
- Migraciones.
- Repositorios o servicios de acceso a datos si hacen falta.
- Integraciones externas en etapas futuras.
- Implementación futura de ARCA/AFIP.
- Implementación futura de PDF/QR.

---

## 6. Entidades principales MVP 1A

### Maestros

- `Cliente`
- `ServicioPlan`
- `ServicioPrecio`
- `CajaDistribucion`
- `Empresa`
- `Usuario` o identidad inicial simple

### Operativas

- `Suscripcion`
- `SuscripcionEstadoHistorial`
- `Factura`
- `FacturaItem`
- `Pago`
- `PagoDetalle`
- `PagoAplicacion`
- `CajaDiaria`
- `MovimientoCaja`

---

## 7. Entidades MVP 1B

- `PuntoVenta`
- `CertificadoEmpresa`
- `FacturaElectronica`
- `AfipToken`

Estas pueden quedar modeladas desde el inicio, pero no deben bloquear el MVP 1A.

---

## 8. Enums principales

```csharp
public enum AmbienteFiscal
{
    Homologacion = 1,
    Produccion = 2
}

public enum CondicionIvaEmpresa
{
    ResponsableInscripto = 1,
    Monotributista = 2,
    Exento = 3
}

public enum TipoDocumentoCliente
{
    SinDocumento = 0,
    DNI = 1,
    CUIT = 2,
    CUIL = 3,
    LE = 4,
    LC = 5
}

public enum RegimenImpositivo
{
    ResponsableInscripto = 1,
    ResponsableNoInscripto = 2,
    Exento = 3,
    ConsumidorFinal = 4,
    Monotributista = 5,
    NoCategorizado = 6
}

public enum TipoServicio
{
    Video = 1,
    Fibra = 2,
    Internet = 3,
    InternetTv = 4,
    Otro = 99
}

public enum EstadoSuscripcionTipo
{
    Activo = 1,
    Baja = 2,
    Suspendido = 3
}

public enum TipoComprobante
{
    FacturaA = 1,
    FacturaB = 6,
    FacturaC = 11,
    FacturaX = 99,
    NotaCreditoA = 3,
    NotaCreditoB = 8,
    NotaDebitoA = 2,
    NotaDebitoB = 7
}

public enum EstadoFiscalFactura
{
    NoRequiereAutorizacion = 0,
    PendienteAutorizacion = 1,
    Autorizada = 2,
    Rechazada = 3,
    Anulada = 4
}

public enum EstadoCobranzaFactura
{
    Pendiente = 1,
    Parcial = 2,
    Pagada = 3,
    Anulada = 4
}

public enum EstadoPago
{
    Registrado = 1,
    Anulado = 2
}

public enum MedioPagoTipo
{
    Efectivo = 1,
    Transferencia = 2,
    Tarjeta = 3,
    Otro = 99
}

public enum EstadoCajaDiaria
{
    Abierta = 1,
    Cerrada = 2
}

public enum TipoMovimientoCaja
{
    Ingreso = 1,
    Egreso = 2,
    Cobro = 3,
    Ajuste = 4
}

public enum TipoPuntoVenta
{
    Electronico = 1,
    Manual = 2
}
```

---

## 9. Flujo principal MVP 1A

### 9.1 Alta de cliente

1. Usuario entra a Clientes.
2. Crea nuevo cliente.
3. Carga nombre, documento/CUIT, régimen impositivo, dirección, teléfono y observaciones.
4. El sistema valida datos mínimos.
5. Guarda cliente.
6. Redirige a ficha del cliente.

### 9.2 Crear suscripción

1. Desde ficha de cliente se crea una suscripción.
2. Se elige servicio/plan.
3. Se elige caja de distribución.
4. Se define fecha de alta.
5. Estado inicial: Activo.
6. Se guarda precio actual como snapshot.
7. Se crea historial inicial de estado.

### 9.3 Generar facturación mensual

1. Usuario selecciona año y mes.
2. Sistema busca suscripciones activas.
3. Excluye las que ya tienen factura para ese período.
4. Muestra preview.
5. Usuario confirma.
6. Sistema crea `Factura` y `FacturaItem`.
7. Estado fiscal inicial:
   - `NoRequiereAutorizacion` para MVP 1A.
   - O `PendienteAutorizacion` si se prepara para MVP 1B.
8. Estado cobranza inicial:
   - `Pendiente`.

### 9.4 Deuda / cobranza rápida

Pantalla clave:

```txt
Buscar cliente -> ver deuda -> seleccionar facturas -> registrar pago
```

Debe permitir:

- Buscar por nombre, código, documento o CUIT.
- Ver facturas pendientes y parciales.
- Seleccionar una o varias facturas.
- Registrar pago.
- Ver saldo restante.

### 9.5 Registrar pago

1. Usuario selecciona facturas.
2. Elige medio de pago.
3. Ingresa monto.
4. Sistema valida caja abierta si corresponde.
5. Crea `Pago`.
6. Crea `PagoDetalle`.
7. Crea una o varias `PagoAplicacion`.
8. Crea `MovimientoCaja`.
9. Actualiza estado de cobranza de las facturas:
   - Pendiente.
   - Parcial.
   - Pagada.

### 9.6 Caja diaria

1. Usuario abre caja con monto inicial.
2. Los cobros generan movimientos.
3. Se pueden registrar egresos manuales.
4. Usuario cierra caja.
5. Sistema calcula monto sistema.
6. Usuario informa monto final.
7. Se guarda diferencia.

---

## 10. Flujo principal MVP 1B

### 10.1 Configuración fiscal

1. Cargar empresa.
2. Cargar punto de venta.
3. Cargar certificado.
4. Configurar ambiente.
5. Probar diagnóstico.

### 10.2 Autorizar factura

1. Usuario abre factura pendiente.
2. Presiona Autorizar ARCA/AFIP.
3. Sistema valida:
   - Empresa.
   - Punto de venta.
   - Certificado.
   - Factura no autorizada.
4. Obtiene token/sign.
5. Arma request WSFEv1.
6. Solicita CAE.
7. Guarda `FacturaElectronica`.
8. Actualiza estado fiscal.

### 10.3 PDF/QR fiscal

1. Usuario abre comprobante.
2. Sistema genera PDF.
3. Incluye QR si tiene CAE.
4. Permite descargar o imprimir.

---

## 11. Reglas de integridad importantes

- No duplicar cliente por código.
- No duplicar CUIT si existe.
- No duplicar documento si existe.
- No duplicar suscripción activa igual.
- No generar dos facturas para la misma suscripción, año y mes.
- No cobrar por encima del total de una factura.
- No aplicar un pago por encima de su total.
- No modificar importes de una factura autorizada.
- No pedir CAE dos veces para la misma factura.
- Solo una caja diaria abierta a la vez.
- Un pago anulado debe revertir aplicaciones y caja.
- La baja comercial de suscripción debe hacerse por estado, no borrando datos.

---

## 12. Pantallas MVP 1A

### Seguridad

- Login.
- Usuarios básicos.

### Configuración inicial

- Empresa.
- Servicios/planes.
- Cajas de distribución.

### Clientes

- Listado de clientes.
- Alta/edición cliente.
- Ficha cliente.
- Suscripciones del cliente.
- Historial de estados.

### Facturación

- Generación mensual.
- Preview de facturación.
- Listado de facturas.
- Detalle de factura.
- Deuda por cliente.
- Deuda general.

### Cobranza

- Cobranza rápida.
- Registrar pago.
- Ver pagos del cliente.

### Caja

- Abrir caja.
- Movimientos de caja.
- Registrar egreso.
- Cerrar caja.
- Reporte diario.

---

## 13. Pantallas MVP 1B

- Configuración fiscal.
- Certificados.
- Punto de venta.
- Diagnóstico ARCA/AFIP.
- Autorizar factura electrónica.
- Ver CAE.
- PDF/QR fiscal.

---

## 14. Orden de desarrollo recomendado

1. Crear solución Blazor.
2. Crear proyectos:
   - `VideoCable.Web`
   - `VideoCable.Domain`
   - `VideoCable.Infrastructure`
3. Agregar referencias entre proyectos.
4. Configurar EF Core y SQL Server.
5. Crear entidades y enums.
6. Crear `AppDbContext`.
7. Configurar `OnModelCreating`.
8. Crear primera migración.
9. Crear base de datos.
10. Seed mínimo:
    - Empresa.
    - Usuario admin.
    - Servicios de prueba.
    - Cajas de prueba.
11. CRUD Servicios.
12. CRUD Cajas de distribución.
13. CRUD Clientes.
14. Ficha cliente.
15. Suscripciones.
16. Historial de estado.
17. Generación mensual de facturas.
18. Pantalla deuda/cobranza rápida.
19. Pagos.
20. Caja diaria.
21. Reportes mínimos.
22. Recién después integrar ARCA/AFIP.

---

## 15. Riesgos

### Riesgo: copiar errores del sistema viejo

Mitigación:

- No mezclar facturación con cobranza.
- No hardcodear punto de venta.
- No hardcodear empresa.
- No meter lógica ARCA/AFIP en la UI.
- No depender de rutas fijas.
- No guardar precios solo en el plan sin snapshot de factura.

### Riesgo: alcance demasiado grande

Mitigación:

- Separar MVP 1A y MVP 1B.
- Dejar compras, stock, contabilidad, portal cliente y notificaciones para después.

### Riesgo: migración de datos inconsistentes

Mitigación:

- Migrar primero clientes, servicios, cajas y suscripciones.
- Migrar deuda pendiente con validación.
- Dejar sistema viejo como consulta histórica.

### Riesgo: ARCA/AFIP trabe el proyecto

Mitigación:

- ARCA/AFIP va en MVP 1B.
- Primero cerrar operación interna.

---

## 16. Preguntas pendientes para el cliente

- ¿Un cliente puede tener más de un servicio activo?
- ¿TV e internet se facturan separados o juntos?
- ¿Se factura mes actual, mes vencido o mes adelantado?
- ¿Qué día vence la factura?
- ¿Aceptan pagos parciales siempre?
- ¿Cobran reconexión o recargo por mora?
- ¿Usan notas de crédito/débito?
- ¿Qué puntos de venta usan?
- ¿Cuánto histórico quieren migrar?
- ¿El sistema viejo puede quedar como consulta?

---

## 17. Criterio de éxito del MVP 1A

El MVP 1A se considera exitoso si permite operar el circuito interno completo durante un mes:

1. Crear clientes.
2. Crear suscripciones.
3. Generar facturas mensuales sin duplicados.
4. Consultar deuda.
5. Registrar pagos parciales o totales.
6. Abrir y cerrar caja.
7. Ver reportes mínimos.

---

## 18. Criterio de éxito del MVP 1B

El MVP 1B se considera exitoso si permite:

1. Configurar certificado y punto de venta.
2. Diagnosticar conexión ARCA/AFIP.
3. Autorizar facturas.
4. Guardar CAE.
5. Generar comprobante fiscal con QR.
6. No mezclar autorización fiscal con cobranza.

---

## 19. Nota final

Este documento será la guía inicial para crear el proyecto. Puede modificarse a medida que aparezcan respuestas del cliente o detalles técnicos nuevos, pero las decisiones centrales deben mantenerse:

```txt
Suscripción como núcleo.
Factura separada de pago.
ARCA/AFIP separado de la UI.
MVP 1A antes que MVP 1B.
```
