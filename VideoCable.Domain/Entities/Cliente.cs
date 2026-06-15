using VideoCable.Domain.Common;
using VideoCable.Domain.Enums;

namespace VideoCable.Domain.Entities;

public class Cliente : AuditableEntity
{
    public int Codigo { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public TipoDocumentoCliente TipoDocumento { get; set; }
    public string? NumeroDocumento { get; set; }

    public string? Cuit { get; set; }

    public RegimenImpositivo RegimenImpositivo { get; set; }

    public string Direccion { get; set; } = string.Empty;
    public string? Localidad { get; set; }

    public string? Telefono { get; set; }
    public string? Email { get; set; }

    public string? Observaciones { get; set; }

    public bool Activo { get; set; } = true;
}