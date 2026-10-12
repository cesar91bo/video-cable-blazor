using System.Globalization;
using System.Text;
using System.Xml.Linq;
using VideoCable.Domain.Entities;
using VideoCable.Domain.Enums;

namespace VideoCable.Infrastructure.Services.Arca;

public sealed class ArcaWsfeClient(IHttpClientFactory factory)
{
    private static readonly XNamespace Ns = "http://ar.gov.afip.dif.FEV1/";
    private static XElement E(string nombre, params object[] contenido) => new(Ns + nombre, contenido);
    public static string? Valor(XContainer xml, string nombre) => xml.Descendants().FirstOrDefault(x => x.Name.LocalName == nombre)?.Value;

    public static XElement Detalle(Factura f, long numero)
    {
        if (f.DocTipoReceptorArca is null || f.DocNroReceptorArca is null || f.CondicionIvaReceptorArca is null)
            throw new InvalidOperationException("Faltan datos fiscales del receptor.");
        var detalle = E("FECAEDetRequest", E("Concepto", 2), E("DocTipo", f.DocTipoReceptorArca.Value), E("DocNro", f.DocNroReceptorArca.Value),
            E("CbteDesde", numero), E("CbteHasta", numero), E("CbteFch", f.FechaEmision.ToString("yyyyMMdd")),
            E("ImpTotal", Decimal(f.Total)), E("ImpTotConc", "0.00"), E("ImpNeto", Decimal(f.Subtotal)),
            E("ImpOpEx", "0.00"), E("ImpTrib", "0.00"), E("ImpIVA", Decimal(f.ImporteIva)),
            E("FchServDesde", new DateTime(f.PeriodoAnio, f.PeriodoMes, 1).ToString("yyyyMMdd")),
            E("FchServHasta", new DateTime(f.PeriodoAnio, f.PeriodoMes, 1).AddMonths(1).AddDays(-1).ToString("yyyyMMdd")),
            E("FchVtoPago", f.FechaVencimiento.ToString("yyyyMMdd")), E("MonId", "PES"), E("MonCotiz", "1.00"),
            E("CondicionIVAReceptorId", f.CondicionIvaReceptorArca.Value));
        if (f.TipoComprobante != TipoComprobante.FacturaC)
            detalle.Add(E("Iva", f.Items.GroupBy(x => x.AlicuotaIva).Select(g => E("AlicIva",
                E("Id", ArcaFiscalRules.CodigoIva(g.Key)), E("BaseImp", Decimal(g.Sum(x => x.Subtotal))), E("Importe", Decimal(g.Sum(x => x.ImporteIva))))).ToArray()));
        return detalle;
    }

    public static XElement Solicitud(Factura f, int pv, long numero) => E("FeCAEReq",
        E("FeCabReq", E("CantReg", 1), E("PtoVta", pv), E("CbteTipo", (int)f.TipoComprobante)), E("FeDetReq", Detalle(f, numero)));

    public async Task<XDocument> Llamar(CredencialesArca auth, AmbienteFiscal ambiente, string metodo, params XElement[] argumentos)
    {
        if (!Enum.IsDefined(ambiente)) throw new InvalidOperationException("Ambiente ARCA no válido.");
        XNamespace soap = "http://schemas.xmlsoap.org/soap/envelope/";
        var requestXml = new XDocument(new XElement(soap + "Envelope", new XAttribute(XNamespace.Xmlns + "soap", soap),
            new XElement(soap + "Body", E(metodo,
                E("Auth", E("Token", auth.Token), E("Sign", auth.Sign), E("Cuit", auth.Cuit)), argumentos))));
        var url = ambiente == AmbienteFiscal.Homologacion ? "https://wswhomo.afip.gov.ar/wsfev1/service.asmx" : "https://servicios1.afip.gov.ar/wsfev1/service.asmx";
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("SOAPAction", Ns.NamespaceName + metodo);
        request.Content = new StringContent(requestXml.ToString(SaveOptions.DisableFormatting), Encoding.UTF8, "text/xml");
        using var response = await factory.CreateClient("Arca").SendAsync(request);
        var contenido = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode) throw new HttpRequestException($"WSFE respondió HTTP {(int)response.StatusCode}. No se puede asumir que la emisión haya fallado; vuelva a consultar el mismo comprobante.");
        var xml = XDocument.Parse(contenido);
        if (xml.Descendants().Any(x => x.Name.LocalName == "Fault")) throw new InvalidOperationException("ARCA devolvió un error SOAP. Revise ambiente, certificado y datos del comprobante.");
        return xml;
    }

    public async Task<long> Ultimo(CredencialesArca auth, AmbienteFiscal ambiente, int pv, int tipo)
    {
        var xml = await Llamar(auth, ambiente, "FECompUltimoAutorizado", E("PtoVta", pv), E("CbteTipo", tipo));
        ComprobarErrores(xml);
        if (!long.TryParse(Valor(xml, "CbteNro"), out var numero) || numero < 0)
            throw new InvalidOperationException("ARCA no devolvió una numeración válida. No se emitió ningún comprobante.");
        return numero;
    }

    public async Task<XElement?> Consultar(CredencialesArca auth, AmbienteFiscal ambiente, int pv, int tipo, long numero)
    {
        var xml = await Llamar(auth, ambiente, "FECompConsultar", E("FeCompConsReq", E("CbteTipo", tipo), E("CbteNro", numero), E("PtoVta", pv)));
        var resultado = xml.Descendants().FirstOrDefault(x => x.Name.LocalName == "ResultGet");
        if (resultado is not null) { ComprobarErrores(xml); return resultado; }
        var errores = Errores(xml);
        if (errores.Count > 0 && errores.All(x => x.Codigo == "602")) return null;
        ComprobarErrores(xml);
        throw new InvalidOperationException("ARCA no devolvió un resultado de consulta válido. Se conserva el intento para recuperar su estado.");
    }

    public Task<XDocument> Emitir(CredencialesArca auth, AmbienteFiscal ambiente, Factura f, int pv, long numero) =>
        Llamar(auth, ambiente, "FECAESolicitar", Solicitud(f, pv, numero));

    public async Task ProbarPuntoVenta(CredencialesArca auth, AmbienteFiscal ambiente, int pv)
    {
        var xml = await Llamar(auth, ambiente, "FEParamGetPtosVenta");
        ValidarPuntoVenta(xml, auth.Cuit, ambiente, pv);
    }

    public static void ValidarPuntoVenta(XContainer xml, long cuit, AmbienteFiscal ambiente, int pv)
    {
        ComprobarErrores(xml);
        var puntos = xml.Descendants().Where(x => x.Name.LocalName == "PtoVenta").ToList();
        var punto = puntos.FirstOrDefault(x => int.TryParse(Valor(x, "Nro"), out var numero) && numero == pv);
        if (punto is null)
        {
            var recibidos = puntos.Count == 0 ? "ninguno" : string.Join(", ", puntos.Select(x => Valor(x, "Nro")));
            throw new InvalidOperationException($"ARCA no devolvió el punto {pv:00000} para CUIT {cuit} en {ambiente}. Puntos devueltos: {recibidos}.");
        }
        var emision = Valor(punto, "EmisionTipo")?.Trim();
        var bloqueado = Valor(punto, "Bloqueado")?.Trim();
        var baja = Valor(punto, "FchBaja")?.Trim();
        // Producción también devuelve la descripción del régimen del emisor.
        var esCae = string.Equals(emision, "CAE", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(emision, "CAE - Ri Iva", StringComparison.OrdinalIgnoreCase);
        if (!esCae ||
            string.Equals(bloqueado, "S", StringComparison.OrdinalIgnoreCase) ||
            !string.IsNullOrWhiteSpace(baja) && !string.Equals(baja, "NULL", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"ARCA devolvió el punto {pv:00000} para CUIT {cuit} en {ambiente}, pero no pasó la validación de CAE. Tipo de emisión: '{emision}'; bloqueado: '{bloqueado}'; fecha de baja: '{baja}'.");
    }

    public static List<(string Codigo, string Mensaje)> Errores(XContainer xml) => xml.Descendants().Where(x => x.Name.LocalName == "Err")
        .Select(x => (Valor(x, "Code") ?? "", Valor(x, "Msg") ?? "Error sin detalle")).ToList();
    public static string Mensajes(XContainer xml) => string.Join(Environment.NewLine, xml.Descendants().Where(x => x.Name.LocalName is "Err" or "Obs")
        .Select(x => $"{Valor(x, "Code")}: {Valor(x, "Msg")}"));
    public static void ComprobarErrores(XContainer xml)
    {
        if (Errores(xml).Count > 0) throw new InvalidOperationException("ARCA: " + Mensajes(xml));
    }
    private static string Decimal(decimal valor) => valor.ToString("0.00", CultureInfo.InvariantCulture);
}
