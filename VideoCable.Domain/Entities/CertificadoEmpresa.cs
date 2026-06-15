using VideoCable.Domain.Common;
using VideoCable.Domain.Enums;

namespace VideoCable.Domain.Entities;

public class CertificadoEmpresa : AuditableEntity
{
    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = default!;

    public AmbienteFiscal Ambiente { get; set; }

    public string StoreLocation { get; set; } = string.Empty;

    public string StoreName { get; set; } = string.Empty;

    public string SerialNumber { get; set; } = string.Empty;

    public string? Subject { get; set; }

    public DateTime? ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }

    public bool Activo { get; set; } = true;
}