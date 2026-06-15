using VideoCable.Domain.Common;

namespace VideoCable.Domain.Entities;

public class CajaDistribucion : AuditableEntity
{
    public string? Codigo { get; set; }

    public string Descripcion { get; set; } = string.Empty;

    public string? Direccion { get; set; }

    public string? Zona { get; set; }

    public decimal? Latitud { get; set; }

    public decimal? Longitud { get; set; }

    public bool Activa { get; set; } = true;
}