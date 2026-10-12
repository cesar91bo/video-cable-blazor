namespace VideoCable.Infrastructure.Services.Arca;

public sealed record EstadoLimiteFacturacion(decimal Limite, decimal Acumulado, decimal ImporteFactura)
{
    public decimal Proyectado => Acumulado + ImporteFactura;
    public decimal Disponible => Math.Max(0, Limite - Acumulado);
    public bool DebeAvisar => Proyectado >= Limite * 0.8m;
    public string Mensaje =>
        (Proyectado > Limite ? "La emisión superaría el límite mensual configurado." :
         Proyectado == Limite ? "La emisión alcanzaría el límite mensual configurado." :
         "Está llegando al límite mensual configurado (80 % o más).") + Environment.NewLine +
        $"Límite: {Limite:C2}. Facturado por ARCA: {Acumulado:C2}. Disponible: {Disponible:C2}." + Environment.NewLine +
        $"Esta factura: {ImporteFactura:C2}. Total previsto: {Proyectado:C2}.";
}

public static class LimiteFacturacionMensual
{
    public static EstadoLimiteFacturacion? Evaluar(decimal? limite, decimal acumulado, decimal importeFactura) =>
        limite is > 0 ? new EstadoLimiteFacturacion(limite.Value, acumulado, importeFactura) : null;
}

public sealed class AvisoLimiteFacturacionException(string mensaje) : Exception(mensaje);
