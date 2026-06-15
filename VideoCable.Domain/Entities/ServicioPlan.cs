using VideoCable.Domain.Common;
using VideoCable.Domain.Enums;

namespace VideoCable.Domain.Entities;

public class ServicioPlan : AuditableEntity
{
    public string? Codigo { get; set; }

    public string Descripcion { get; set; } = string.Empty;

    public TipoServicio TipoServicio { get; set; }

    public TipoCobroServicio TipoCobro { get; set; } = TipoCobroServicio.Mensual;

    public decimal PrecioActual { get; set; }

    public bool Activo { get; set; } = true;
}