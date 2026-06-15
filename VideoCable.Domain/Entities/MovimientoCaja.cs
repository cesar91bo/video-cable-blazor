using VideoCable.Domain.Common;
using VideoCable.Domain.Enums;

namespace VideoCable.Domain.Entities;

public class MovimientoCaja : AuditableEntity
{
    public int CajaDiariaId { get; set; }
    public CajaDiaria CajaDiaria { get; set; } = default!;

    public TipoMovimientoCaja Tipo { get; set; }

    public DateTime Fecha { get; set; }

    public decimal Monto { get; set; }

    public string Concepto { get; set; } = string.Empty;

    public int? PagoId { get; set; }
    public Pago? Pago { get; set; }

    public int? UsuarioId { get; set; }

    public bool Anulado { get; set; }

    public DateTime? FechaAnulacion { get; set; }

    public string? MotivoAnulacion { get; set; }
}