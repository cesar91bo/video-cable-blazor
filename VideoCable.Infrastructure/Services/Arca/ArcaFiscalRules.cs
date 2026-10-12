using VideoCable.Domain.Entities;
using VideoCable.Domain.Enums;

namespace VideoCable.Infrastructure.Services.Arca;

public sealed record ReceptorArca(int TipoDocumento, long NumeroDocumento, int CondicionIva);

public static class ArcaFiscalRules
{
    // RG 5700/2025, importe vigente verificado en ARCA el 11/10/2026.
    public const decimal UmbralIdentificacionConsumidorFinal = 10_000_000m;
    public static bool CuitValido(string? texto)
    {
        var numero = SoloNumeros(texto);
        if (numero.Length != 11 || numero.All(c => c == '0')) return false;
        int[] pesos = [5, 4, 3, 2, 7, 6, 5, 4, 3, 2];
        var digito = 11 - Enumerable.Range(0, 10).Sum(i => (numero[i] - '0') * pesos[i]) % 11;
        digito = digito == 11 ? 0 : digito == 10 ? 9 : digito;
        return digito == numero[10] - '0';
    }

    public static string SoloNumeros(string? texto) => new((texto ?? "").Where(c => c >= '0' && c <= '9').ToArray());

    public static ReceptorArca Receptor(Cliente cliente, TipoComprobante tipoFactura, decimal total)
    {
        var condicion = cliente.RegimenImpositivo switch
        {
            RegimenImpositivo.ResponsableInscripto => 1,
            RegimenImpositivo.Exento => 4,
            RegimenImpositivo.ConsumidorFinal => 5,
            RegimenImpositivo.Monotributista => 6,
            RegimenImpositivo.NoCategorizado => 7,
            _ => throw new InvalidOperationException("Actualice la condición IVA del cliente. Responsable no inscripto no está habilitado en este flujo.")
        };
        var documentoInformado = SoloNumeros(cliente.NumeroDocumento);
        var sinDocumento = string.IsNullOrWhiteSpace(cliente.Cuit) &&
            (documentoInformado.Length == 0 || documentoInformado.All(c => c == '0'));
        if (condicion == 5 && tipoFactura is TipoComprobante.FacturaB or TipoComprobante.FacturaC && sinDocumento)
        {
            if (total >= UmbralIdentificacionConsumidorFinal)
                throw new InvalidOperationException("Para operaciones a consumidor final de $10.000.000 o más debe cargar el documento del cliente.");
            return new ReceptorArca(99, 0, condicion);
        }
        var tipo = !string.IsNullOrWhiteSpace(cliente.Cuit) ? 80 : cliente.TipoDocumento switch
        {
            TipoDocumentoCliente.CUIT => 80,
            TipoDocumentoCliente.CUIL => 86,
            TipoDocumentoCliente.DNI => 96,
            TipoDocumentoCliente.LE => 89,
            TipoDocumentoCliente.LC => 90,
            _ => throw new InvalidOperationException("Cargue DNI, CUIT o CUIL del cliente antes de emitir una factura fiscal.")
        };
        var numero = SoloNumeros(tipo == 80 && !string.IsNullOrWhiteSpace(cliente.Cuit) ? cliente.Cuit : cliente.NumeroDocumento);
        if (tipo is 80 or 86)
        {
            if (!CuitValido(numero)) throw new InvalidOperationException("El CUIT/CUIL del cliente no es válido.");
        }
        else if (numero.Length is < 1 or > 8 || !long.TryParse(numero, out var n) || n <= 0)
            throw new InvalidOperationException("El documento del cliente debe contener de 1 a 8 dígitos y ser mayor a cero.");
        if (condicion is 1 or 4 or 6 or 7 && tipo != 80)
            throw new InvalidOperationException("La condición IVA seleccionada requiere CUIT del cliente.");
        return new ReceptorArca(tipo, long.Parse(numero), condicion);
    }

    public static TipoComprobante TipoFactura(CondicionIvaEmpresa emisor, RegimenImpositivo receptor) => emisor switch
    {
        CondicionIvaEmpresa.ResponsableInscripto => receptor is RegimenImpositivo.ResponsableInscripto or RegimenImpositivo.Monotributista
            ? TipoComprobante.FacturaA : TipoComprobante.FacturaB,
        CondicionIvaEmpresa.Monotributista or CondicionIvaEmpresa.Exento => TipoComprobante.FacturaC,
        _ => throw new InvalidOperationException("Condición IVA de la empresa no válida.")
    };

    public static int CodigoIva(decimal tasa) => tasa switch
    {
        0m => 3,
        2.5m => 9,
        5m => 8,
        10.5m => 4,
        21m => 5,
        27m => 6,
        _ => throw new InvalidOperationException("Alícuota IVA no soportada. Use 0, 2,5, 5, 10,5, 21 o 27 %.")
    };

    public static void Preparar(Factura factura, DateTime fecha)
    {
        if (!CuitValido(factura.Empresa.Cuit)) throw new InvalidOperationException("El CUIT de la empresa no es válido.");
        if (factura.Items.Count == 0 || factura.Items.Any(x => x.Cantidad <= 0 || x.Total <= 0 || decimal.Round(x.Total, 2) != x.Total))
            throw new InvalidOperationException("La factura debe tener ítems con cantidades e importes positivos y hasta dos decimales.");
        if (Math.Abs(factura.Items.Sum(x => x.Total) - factura.Total) > 0.01m)
            throw new InvalidOperationException("El total de los ítems no coincide con el total de la factura.");
        var tipoFactura = TipoFactura(factura.Empresa.CondicionIva, factura.Cliente.RegimenImpositivo);
        var receptor = Receptor(factura.Cliente, tipoFactura, factura.Total);
        factura.TipoComprobante = tipoFactura;
        foreach (var item in factura.Items)
        {
            var tasa = factura.TipoComprobante == TipoComprobante.FacturaC ? 0m :
                item.AlicuotaIva > 0 ? item.AlicuotaIva : factura.Empresa.AlicuotaIvaArca;
            CodigoIva(tasa);
            item.AlicuotaIva = tasa;
            item.Subtotal = decimal.Round(item.Total / (1 + tasa / 100m), 2, MidpointRounding.AwayFromZero);
            item.ImporteIva = item.Total - item.Subtotal;
            item.PrecioUnitario = decimal.Round(item.Subtotal / item.Cantidad, 2, MidpointRounding.AwayFromZero);
        }
        factura.Subtotal = factura.Items.Sum(x => x.Subtotal);
        factura.ImporteIva = factura.Items.Sum(x => x.ImporteIva);
        factura.FechaEmision = fecha.Date;
        if (factura.FechaVencimiento < factura.FechaEmision) factura.FechaVencimiento = fecha.Date.AddDays(10);
        factura.AmbienteArca = factura.Empresa.AmbienteArca;
        factura.DocTipoReceptorArca = receptor.TipoDocumento;
        factura.DocNroReceptorArca = receptor.NumeroDocumento;
        factura.CondicionIvaReceptorArca = receptor.CondicionIva;
        factura.ClienteNombreSnapshot = factura.Cliente.Nombre;
        factura.ClienteCuitSnapshot = receptor.TipoDocumento == 80 ? receptor.NumeroDocumento.ToString() : null;
        factura.ClienteDocumentoSnapshot = receptor.TipoDocumento is not (80 or 99) ? receptor.NumeroDocumento.ToString() : null;
        factura.ClienteCondicionIvaSnapshot = factura.Cliente.RegimenImpositivo.ToString();
        factura.ClienteDireccionSnapshot = factura.Cliente.Direccion;
        factura.EmpresaCuitSnapshot = SoloNumeros(factura.Empresa.Cuit);
        factura.EmpresaRazonSocialSnapshot = factura.Empresa.RazonSocial;
        factura.EmpresaCondicionIvaSnapshot = factura.Empresa.CondicionIva.ToString();
        factura.EmpresaDireccionSnapshot = factura.Empresa.Direccion;
        factura.EstadoFiscal = EstadoFiscalFactura.PendienteAutorizacion;
    }

    public static void ValidarFecha(Factura factura, DateTime hoy)
    {
        if (factura.FechaEmision.Date < hoy.Date.AddDays(-10) || factura.FechaEmision.Date > hoy.Date.AddDays(10))
            throw new InvalidOperationException("La fecha de emisión de servicios debe estar dentro de los 10 días anteriores o posteriores a hoy. Revise la factura antes de emitir.");
        if (factura.PeriodoMes is < 1 or > 12 || factura.PeriodoAnio is < 2000 or > 2100)
            throw new InvalidOperationException("Período del servicio no válido.");
        if (factura.FechaVencimiento.Date < factura.FechaEmision.Date)
            throw new InvalidOperationException("El vencimiento no puede ser anterior a la emisión.");
    }
}
