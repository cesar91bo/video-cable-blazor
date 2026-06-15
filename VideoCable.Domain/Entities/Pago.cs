using VideoCable.Domain.Common;
using VideoCable.Domain.Enums;

namespace VideoCable.Domain.Entities;

public class Pago : AuditableEntity
{
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = default!;

    public int? CajaDiariaId { get; set; }
    public CajaDiaria? CajaDiaria { get; set; }

    public DateTime Fecha { get; set; }

    public decimal Total { get; set; }

    public int? UsuarioId { get; set; }

    public string? Observaciones { get; set; }

    public EstadoPago Estado { get; set; } = EstadoPago.Registrado;

    public ICollection<PagoDetalle> Detalles { get; set; } = new List<PagoDetalle>();
    public ICollection<PagoAplicacion> Aplicaciones { get; set; } = new List<PagoAplicacion>();
}