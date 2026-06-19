using System.ComponentModel.DataAnnotations;

namespace VideoCable.Domain.Enums;

public enum TipoDocumentoCliente
{
    [Display(Name = "Sin documento")]
    SinDocumento = 0,
    DNI = 1,
    CUIT = 2,
    CUIL = 3,
    LE = 4,
    LC = 5
}

public enum RegimenImpositivo
{
    [Display(Name = "Responsable Inscripto")]
    ResponsableInscripto = 1,
    [Display(Name = "Responsable No Inscripto")]
    ResponsableNoInscripto = 2,
    Exento = 3,
    [Display(Name = "Consumidor Final")]
    ConsumidorFinal = 4,
    Monotributista = 5,
    [Display(Name = "No Categorizado")]
    NoCategorizado = 6
}

public enum TipoServicio
{
    Video = 1,
    Fibra = 2,
    Internet = 3,
    [Display(Name = "Internet + TV")]
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
    [Display(Name = "Factura A")]
    FacturaA = 1,
    [Display(Name = "Factura B")]
    FacturaB = 6,
    [Display(Name = "Factura C")]
    FacturaC = 11,
    [Display(Name = "Factura X")] 
    FacturaX = 99,
    [Display(Name = "Nota de Crédito A")]
    NotaCreditoA = 3,
    [Display(Name = "Nota de Crédito B")]
    NotaCreditoB = 8,
    [Display(Name = "Nota de Débito A")]
    NotaDebitoA = 2,
    [Display(Name = "Nota de Débito B")]
    NotaDebitoB = 7
}

public enum EstadoFiscalFactura
{
    [Display(Name = "No Requiere Autorización")]
    NoRequiereAutorizacion = 0,
    [Display(Name = "Pendiente de Autorización")]
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

public enum TipoCobroServicio
{
    Mensual = 1,
    Puntual = 2
}

public enum EstadoReclamo
{
    Abierto = 1,
    EnProceso = 2,
    Resuelto = 3,
    Cerrado = 4,
    Cancelado = 5
}

public enum TipoReclamo
{
    SinSenal = 1,
    InternetLento = 2,
    CorteServicio = 3,
    Facturacion = 4,
    ReclamoTecnico = 5,
    Otro = 99
}

public enum PrioridadReclamo
{
    Baja = 1,
    Media = 2,
    Alta = 3,
    Urgente = 4
}