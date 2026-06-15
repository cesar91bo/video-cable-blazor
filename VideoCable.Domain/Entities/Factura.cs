using VideoCable.Domain.Common;
using VideoCable.Domain.Enums;

namespace VideoCable.Domain.Entities;

public class Factura : AuditableEntity
{
    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = default!;

    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = default!;

    public int SuscripcionId { get; set; }
    public Suscripcion Suscripcion { get; set; } = default!;

    public int PeriodoAnio { get; set; }
    public int PeriodoMes { get; set; }

    public DateTime FechaEmision { get; set; }
    public DateTime FechaVencimiento { get; set; }

    public TipoComprobante TipoComprobante { get; set; }

    public int? PuntoVentaId { get; set; }
    public int? PuntoVentaNumeroSnapshot { get; set; }

    public long? NumeroComprobante { get; set; }
    public string? NumeroComprobanteFormateado { get; set; }

    public decimal Subtotal { get; set; }
    public decimal ImporteIva { get; set; }
    public decimal Total { get; set; }

    public EstadoFiscalFactura EstadoFiscal { get; set; } = EstadoFiscalFactura.NoRequiereAutorizacion;
    public EstadoCobranzaFactura EstadoCobranza { get; set; } = EstadoCobranzaFactura.Pendiente;

    public string? Observaciones { get; set; }

    // Snapshots cliente
    public string ClienteNombreSnapshot { get; set; } = string.Empty;
    public string? ClienteDocumentoSnapshot { get; set; }
    public string? ClienteCuitSnapshot { get; set; }
    public string ClienteCondicionIvaSnapshot { get; set; } = string.Empty;
    public string? ClienteDireccionSnapshot { get; set; }

    // Snapshots servicio
    public string ServicioDescripcionSnapshot { get; set; } = string.Empty;
    public string ServicioTipoSnapshot { get; set; } = string.Empty;
    public string? CajaDescripcionSnapshot { get; set; }

    // Snapshots empresa
    public string EmpresaRazonSocialSnapshot { get; set; } = string.Empty;
    public string EmpresaCuitSnapshot { get; set; } = string.Empty;
    public string EmpresaCondicionIvaSnapshot { get; set; } = string.Empty;
    public string EmpresaDireccionSnapshot { get; set; } = string.Empty;

    public ICollection<FacturaItem> Items { get; set; } = new List<FacturaItem>();
    public ICollection<PagoAplicacion> PagoAplicaciones { get; set; } = new List<PagoAplicacion>();
}