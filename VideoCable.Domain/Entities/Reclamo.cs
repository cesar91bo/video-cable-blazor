using VideoCable.Domain.Common;
using VideoCable.Domain.Enums;

namespace VideoCable.Domain.Entities;

public class Reclamo : AuditableEntity
{
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public int? SuscripcionId { get; set; }
    public Suscripcion? Suscripcion { get; set; }

    public int? CajaDistribucionId { get; set; }
    public CajaDistribucion? CajaDistribucion { get; set; }

    public DateTime FechaReclamo { get; set; } = DateTime.Now;

    public TipoReclamo Tipo { get; set; } = TipoReclamo.Otro;

    public PrioridadReclamo Prioridad { get; set; } = PrioridadReclamo.Media;

    public EstadoReclamo Estado { get; set; } = EstadoReclamo.Abierto;

    public string Descripcion { get; set; } = string.Empty;

    public string? ObservacionesInternas { get; set; }

    public string? Resolucion { get; set; }

    public DateTime? FechaResolucion { get; set; }

    public DateTime? FechaCierre { get; set; }
}