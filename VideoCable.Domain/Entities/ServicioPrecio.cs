using VideoCable.Domain.Common;

namespace VideoCable.Domain.Entities;

public class ServicioPrecio : AuditableEntity
{
    public int ServicioPlanId { get; set; }
    public ServicioPlan ServicioPlan { get; set; } = default!;

    public decimal Precio { get; set; }

    public DateTime VigenteDesde { get; set; }

    public DateTime? VigenteHasta { get; set; }

    public string? Motivo { get; set; }
}