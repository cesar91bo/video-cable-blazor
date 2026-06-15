using VideoCable.Domain.Common;
using VideoCable.Domain.Enums;

namespace VideoCable.Domain.Entities;

public class Empresa : AuditableEntity
{
    public string RazonSocial { get; set; } = string.Empty;
    public string? NombreFantasia { get; set; }

    public string Cuit { get; set; } = string.Empty;
    public CondicionIvaEmpresa CondicionIva { get; set; }

    public string? IngresosBrutos { get; set; }
    public DateTime? InicioActividades { get; set; }

    public string Direccion { get; set; } = string.Empty;
    public string? Localidad { get; set; }
    public string? Provincia { get; set; }

    public string? Telefono { get; set; }
    public string? Email { get; set; }

    public bool Activa { get; set; } = true;
}