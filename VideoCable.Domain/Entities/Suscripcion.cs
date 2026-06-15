using VideoCable.Domain.Common;
using VideoCable.Domain.Enums;

namespace VideoCable.Domain.Entities;

public class Suscripcion : AuditableEntity
{
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = default!;

    public int ServicioPlanId { get; set; }
    public ServicioPlan ServicioPlan { get; set; } = default!;

    public int CajaDistribucionId { get; set; }
    public CajaDistribucion CajaDistribucion { get; set; } = default!;

    public EstadoSuscripcionTipo EstadoActual { get; set; } = EstadoSuscripcionTipo.Activo;

    public DateTime FechaAlta { get; set; }

    public DateTime? FechaBaja { get; set; }

    public decimal PrecioActualSnapshot { get; set; }

    public string? Observaciones { get; set; }
}