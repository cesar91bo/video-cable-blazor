using VideoCable.Domain.Enums;

namespace VideoCable.Domain.Entities;

public class SuscripcionEstadoHistorial
{
    public int Id { get; set; }

    public int SuscripcionId { get; set; }
    public Suscripcion Suscripcion { get; set; } = default!;

    public EstadoSuscripcionTipo Estado { get; set; }

    public DateTime Fecha { get; set; }

    public int? UsuarioId { get; set; }

    public string? Observaciones { get; set; }
}