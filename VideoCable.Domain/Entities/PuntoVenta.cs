using VideoCable.Domain.Common;
using VideoCable.Domain.Enums;

namespace VideoCable.Domain.Entities;

public class PuntoVenta : AuditableEntity
{
    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = default!;

    public int Numero { get; set; }

    public string Descripcion { get; set; } = string.Empty;

    public AmbienteFiscal Ambiente { get; set; }

    public TipoPuntoVenta Tipo { get; set; }

    public bool Activo { get; set; } = true;

    public bool Predeterminado { get; set; }
}