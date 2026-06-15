namespace VideoCable.Domain.Enums;

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

public enum TipoCobroServicio
{
    Mensual = 1,
    Puntual = 2
}