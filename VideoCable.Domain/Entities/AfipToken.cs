using VideoCable.Domain.Enums;

namespace VideoCable.Domain.Entities;

public class AfipToken
{
    public int Id { get; set; }

    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = default!;

    public AmbienteFiscal Ambiente { get; set; }

    public string Servicio { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;

    public string Sign { get; set; } = string.Empty;

    public DateTime FechaDesde { get; set; }

    public DateTime FechaHasta { get; set; }

    public DateTime CreatedAt { get; set; }
}