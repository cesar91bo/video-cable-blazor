using VideoCable.Domain.Enums;

namespace VideoCable.Domain.Entities;

public class PagoDetalle
{
    public int Id { get; set; }

    public int PagoId { get; set; }
    public Pago Pago { get; set; } = default!;

    public MedioPagoTipo MedioPago { get; set; }

    public decimal Monto { get; set; }

    public string? Referencia { get; set; }
}