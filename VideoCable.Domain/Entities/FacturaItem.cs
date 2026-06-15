namespace VideoCable.Domain.Entities;

public class FacturaItem
{
    public int Id { get; set; }

    public int FacturaId { get; set; }
    public Factura Factura { get; set; } = default!;

    public int? SuscripcionId { get; set; }
    public Suscripcion? Suscripcion { get; set; }

    public int? ServicioPlanId { get; set; }
    public ServicioPlan? ServicioPlan { get; set; }

    public string Descripcion { get; set; } = string.Empty;

    public decimal Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal AlicuotaIva { get; set; }

    public decimal Subtotal { get; set; }

    public decimal ImporteIva { get; set; }

    public decimal Total { get; set; }
}