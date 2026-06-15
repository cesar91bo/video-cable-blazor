using VideoCable.Domain.Common;

namespace VideoCable.Domain.Entities;

public class FacturaElectronica : AuditableEntity
{
    public int FacturaId { get; set; }
    public Factura Factura { get; set; } = default!;

    public string CAE { get; set; } = string.Empty;

    public DateTime FechaVencimientoCAE { get; set; }

    public DateTime FechaAutorizacion { get; set; }

    public string Resultado { get; set; } = string.Empty;

    public string? Observaciones { get; set; }

    public string? Errores { get; set; }

    public string? RequestResumen { get; set; }

    public string? ResponseResumen { get; set; }

    public string? QrUrl { get; set; }
}