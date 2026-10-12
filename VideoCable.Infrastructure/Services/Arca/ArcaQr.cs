using System.Text;
using System.Text.Json;
using QRCoder;
using VideoCable.Domain.Entities;

namespace VideoCable.Infrastructure.Services.Arca;

public static class ArcaQr
{
    public static string CrearUrl(Factura factura, string cae)
    {
        if (factura.NumeroComprobante is null || factura.PuntoVentaNumeroSnapshot is null)
            throw new InvalidOperationException("Falta numeración fiscal para generar el QR.");
        var datos = new Dictionary<string, object>
        {
            ["ver"] = 1,
            ["fecha"] = factura.FechaEmision.ToString("yyyy-MM-dd"),
            ["cuit"] = long.Parse(ArcaFiscalRules.SoloNumeros(factura.EmpresaCuitSnapshot)),
            ["ptoVta"] = factura.PuntoVentaNumeroSnapshot.Value,
            ["tipoCmp"] = (int)factura.TipoComprobante,
            ["nroCmp"] = factura.NumeroComprobante.Value,
            ["importe"] = factura.Total,
            ["moneda"] = "PES",
            ["ctz"] = 1m,
            ["tipoCodAut"] = "E",
            ["codAut"] = long.Parse(cae)
        };
        if (factura.DocTipoReceptorArca is not null && factura.DocNroReceptorArca is not null)
        {
            datos["tipoDocRec"] = factura.DocTipoReceptorArca.Value;
            datos["nroDocRec"] = factura.DocNroReceptorArca.Value;
        }
        return "https://www.arca.gob.ar/fe/qr/?p=" + Uri.EscapeDataString(Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(datos))));
    }

    public static string Imagen(string url) => "data:image/png;base64," + Convert.ToBase64String(
        PngByteQRCodeHelper.GetQRCode(url, QRCodeGenerator.ECCLevel.M, 4));
}
