namespace VideoCable.Domain.Entities;

public class PagoAplicacion
{
    public int Id { get; set; }

    public int PagoId { get; set; }
    public Pago Pago { get; set; } = default!;

    public int FacturaId { get; set; }
    public Factura Factura { get; set; } = default!;

    public decimal MontoAplicado { get; set; }
}