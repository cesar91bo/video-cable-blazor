namespace VideoCable.Web.Services;

public sealed record MovimientoReporte(int Id, DateTime Fecha, int ClienteId, string Cliente,
    string Concepto, decimal Importe);

public sealed record FilaReporte(string Etiqueta, int Operaciones, decimal Importe);

public static class ResumenReporte
{
    public static List<MovimientoReporte> Desglosar(MovimientoReporte movimiento,
        IEnumerable<(string Concepto, decimal Importe)> partes, string conceptoRestante)
    {
        var detalle = partes.Select(x => movimiento with { Concepto = x.Concepto, Importe = x.Importe }).ToList();
        var restante = movimiento.Importe - detalle.Sum(x => x.Importe);
        if (restante != 0)
            detalle.Add(movimiento with { Concepto = conceptoRestante, Importe = restante });
        return detalle;
    }

    public static DateTime InicioSemana(DateTime fecha) =>
        fecha.Date.AddDays(-(((int)fecha.DayOfWeek + 6) % 7));

    public static List<FilaReporte> Agrupar(IEnumerable<MovimientoReporte> movimientos, string agrupacion)
    {
        if (agrupacion is "dia" or "semana" or "mes")
        {
            return movimientos.GroupBy(x => agrupacion switch
                {
                    "semana" => InicioSemana(x.Fecha),
                    "mes" => new DateTime(x.Fecha.Year, x.Fecha.Month, 1),
                    _ => x.Fecha.Date
                })
                .OrderBy(x => x.Key)
                .Select(g => new FilaReporte(agrupacion switch
                {
                    "semana" => $"{g.Key:dd/MM/yyyy} al {g.Key.AddDays(6):dd/MM/yyyy}",
                    "mes" => g.Key.ToString("MM/yyyy"),
                    _ => g.Key.ToString("dd/MM/yyyy")
                }, g.Select(x => x.Id).Distinct().Count(), g.Sum(x => x.Importe)))
                .ToList();
        }

        return movimientos.GroupBy(x => agrupacion == "cliente"
                ? $"{x.ClienteId}|{x.Cliente}" : x.Concepto)
            .Select(g => new FilaReporte(agrupacion == "cliente"
                ? g.First().Cliente : g.Key,
                g.Select(x => x.Id).Distinct().Count(), g.Sum(x => x.Importe)))
            .OrderByDescending(x => x.Importe).ThenBy(x => x.Etiqueta).ToList();
    }
}
