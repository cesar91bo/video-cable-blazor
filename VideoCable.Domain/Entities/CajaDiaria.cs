using VideoCable.Domain.Common;
using VideoCable.Domain.Enums;

namespace VideoCable.Domain.Entities;

public class CajaDiaria : AuditableEntity
{
    public DateTime FechaApertura { get; set; }

    public int? UsuarioAperturaId { get; set; }

    public decimal MontoInicial { get; set; }

    public DateTime? FechaCierre { get; set; }

    public int? UsuarioCierreId { get; set; }

    public decimal? MontoFinalInformado { get; set; }

    public decimal? MontoSistema { get; set; }

    public decimal? Diferencia { get; set; }

    public EstadoCajaDiaria Estado { get; set; } = EstadoCajaDiaria.Abierta;

    public string? Observaciones { get; set; }

    public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    public ICollection<MovimientoCaja> Movimientos { get; set; } = new List<MovimientoCaja>();
}