using System.Globalization;
using System.Text;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using VideoCable.Domain.Enums;
using VideoCable.Infrastructure.Data;
using VideoCable.Domain.Entities;

namespace VideoCable.Infrastructure.Services.Arca;

public class ArcaFacturaService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpClientFactory _httpClientFactory;

    public ArcaFacturaService(
        AppDbContext dbContext,
        IHttpClientFactory httpClientFactory)
    {
        _dbContext = dbContext;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string> ConsultarUltimoComprobanteAsync(
        int empresaId,
        AmbienteFiscal ambiente,
        int puntoVenta,
        int tipoComprobante)
    {
        var auth = await ObtenerAuthAsync(empresaId, ambiente, "wsfe");

        var soap = $"""
        <?xml version="1.0" encoding="utf-8"?>
        <soap:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                       xmlns:xsd="http://www.w3.org/2001/XMLSchema"
                       xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
          <soap:Body>
            <FECompUltimoAutorizado xmlns="http://ar.gov.afip.dif.FEV1/">
              <Auth>
                <Token>{auth.Token}</Token>
                <Sign>{auth.Sign}</Sign>
                <Cuit>{auth.Cuit}</Cuit>
              </Auth>
              <PtoVta>{puntoVenta}</PtoVta>
              <CbteTipo>{tipoComprobante}</CbteTipo>
            </FECompUltimoAutorizado>
          </soap:Body>
        </soap:Envelope>
        """;

        var respuesta = await LlamarWsfeAsync(
            soap,
            "http://ar.gov.afip.dif.FEV1/FECompUltimoAutorizado",
            ambiente);

        var xml = XDocument.Parse(respuesta);

        var cbteNro = xml
            .Descendants()
            .FirstOrDefault(x => x.Name.LocalName == "CbteNro")
            ?.Value;

        var errores = LeerErrores(xml);

        if (!string.IsNullOrWhiteSpace(errores))
            return $"ARCA devolvió errores:{Environment.NewLine}{errores}";

        return $"Último comprobante autorizado para PV {puntoVenta}, Tipo {tipoComprobante}: {cbteNro}";
    }

    public async Task<string> SolicitarCaeFacturaBPruebaAsync(
        int empresaId,
        AmbienteFiscal ambiente,
        int puntoVenta)
    {
        const int tipoComprobanteFacturaB = 6;

        var auth = await ObtenerAuthAsync(empresaId, ambiente, "wsfe");

        var ultimo = await ObtenerUltimoNumeroAsync(
            auth,
            ambiente,
            puntoVenta,
            tipoComprobanteFacturaB);

        var proximoNumero = ultimo + 1;

        var hoy = DateTime.Today;
        var fecha = hoy.ToString("yyyyMMdd");

        decimal total = 1000m;
        decimal neto = Math.Round(total / 1.21m, 2);
        decimal iva = total - neto;

        var totalStr = FormatoDecimal(total);
        var netoStr = FormatoDecimal(neto);
        var ivaStr = FormatoDecimal(iva);

        var soap = $"""
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
               xmlns:xsd="http://www.w3.org/2001/XMLSchema"
               xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <FECAESolicitar xmlns="http://ar.gov.afip.dif.FEV1/">
      <Auth>
        <Token>{auth.Token}</Token>
        <Sign>{auth.Sign}</Sign>
        <Cuit>{auth.Cuit}</Cuit>
      </Auth>
      <FeCAEReq>
        <FeCabReq>
          <CantReg>1</CantReg>
          <PtoVta>{puntoVenta}</PtoVta>
          <CbteTipo>{tipoComprobanteFacturaB}</CbteTipo>
        </FeCabReq>
        <FeDetReq>
          <FECAEDetRequest>
            <Concepto>2</Concepto>
            <DocTipo>99</DocTipo>
            <DocNro>0</DocNro>
            <CbteDesde>{proximoNumero}</CbteDesde>
            <CbteHasta>{proximoNumero}</CbteHasta>
            <CbteFch>{fecha}</CbteFch>
            <ImpTotal>{totalStr}</ImpTotal>
            <ImpTotConc>0.00</ImpTotConc>
            <ImpNeto>{netoStr}</ImpNeto>
            <ImpOpEx>0.00</ImpOpEx>
            <ImpTrib>0.00</ImpTrib>
            <ImpIVA>{ivaStr}</ImpIVA>
            <FchServDesde>{fecha}</FchServDesde>
            <FchServHasta>{fecha}</FchServHasta>
            <FchVtoPago>{fecha}</FchVtoPago>
            <MonId>PES</MonId>
            <MonCotiz>1.00</MonCotiz>
            <CondicionIVAReceptorId>5</CondicionIVAReceptorId>
            <Iva>
              <AlicIva>
                <Id>5</Id>
                <BaseImp>{netoStr}</BaseImp>
                <Importe>{ivaStr}</Importe>
              </AlicIva>
            </Iva>
          </FECAEDetRequest>
        </FeDetReq>
      </FeCAEReq>
    </FECAESolicitar>
  </soap:Body>
</soap:Envelope>
""";

        var respuesta = await LlamarWsfeAsync(
            soap,
            "http://ar.gov.afip.dif.FEV1/FECAESolicitar",
            ambiente);

        var xml = XDocument.Parse(respuesta);

        var errores = LeerErrores(xml);
        var observaciones = LeerObservaciones(xml);

        var resultado = xml
            .Descendants()
            .FirstOrDefault(x => x.Name.LocalName == "Resultado")
            ?.Value;

        var cae = xml
            .Descendants()
            .FirstOrDefault(x => x.Name.LocalName == "CAE")
            ?.Value;

        var caeVto = xml
            .Descendants()
            .FirstOrDefault(x => x.Name.LocalName == "CAEFchVto")
            ?.Value;

        if (!string.IsNullOrWhiteSpace(errores))
        {
            return
                "ARCA rechazó la solicitud." + Environment.NewLine +
                errores + Environment.NewLine +
                observaciones;
        }

        return
            "CAE solicitado correctamente en HOMOLOGACIÓN." + Environment.NewLine +
            $"Resultado: {resultado}" + Environment.NewLine +
            $"Punto de venta: {puntoVenta}" + Environment.NewLine +
            $"Tipo comprobante: Factura B ({tipoComprobanteFacturaB})" + Environment.NewLine +
            $"Número: {proximoNumero}" + Environment.NewLine +
            $"Importe total: {total:C2}" + Environment.NewLine +
            $"CAE: {cae}" + Environment.NewLine +
            $"Vencimiento CAE: {caeVto}" + Environment.NewLine +
            observaciones;
    }

    private async Task GuardarResultadoFacturaElectronicaAsync(
    Factura factura,
    string resultado,
    string? cae,
    DateTime? fechaVencimientoCae,
    string? observaciones,
    string? errores,
    string requestResumen,
    string responseResumen)
    {
        var facturaElectronica = await _dbContext.FacturasElectronicas
            .FirstOrDefaultAsync(x => x.FacturaId == factura.Id);

        if (facturaElectronica is null)
        {
            facturaElectronica = new FacturaElectronica
            {
                FacturaId = factura.Id,
                CreatedAt = DateTime.Now
            };

            _dbContext.FacturasElectronicas.Add(facturaElectronica);
        }

        facturaElectronica.CAE = cae ?? string.Empty;
        facturaElectronica.FechaVencimientoCAE = fechaVencimientoCae ?? DateTime.MinValue;
        facturaElectronica.FechaAutorizacion = DateTime.Now;
        facturaElectronica.Resultado = resultado;
        facturaElectronica.Observaciones = observaciones;
        facturaElectronica.Errores = errores;
        facturaElectronica.RequestResumen = requestResumen;
        facturaElectronica.ResponseResumen = responseResumen;
        facturaElectronica.UpdatedAt = DateTime.Now;
    }

    public async Task<string> AutorizarFacturaAsync(
    int facturaId,
    AmbienteFiscal ambiente)
    {
        const int condicionIvaConsumidorFinal = 5;

        var factura = await _dbContext.Facturas
            .Include(x => x.Empresa)
            .Include(x => x.Cliente)
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == facturaId);

        if (factura is null)
            return $"No se encontró la factura Id {facturaId}.";

        if (factura.TipoComprobante == TipoComprobante.FacturaX)
            return "La factura es tipo X. No requiere autorización ARCA.";

        if (factura.EstadoFiscal == EstadoFiscalFactura.Autorizada)
            return "La factura ya está autorizada por ARCA.";

        var puntoVenta = await _dbContext.PuntosVenta
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.EmpresaId == factura.EmpresaId &&
                x.Ambiente == ambiente &&
                x.Activo &&
                x.Predeterminado &&
                !x.IsDeleted);

        if (puntoVenta is null)
            return "No se encontró un punto de venta electrónico predeterminado para el ambiente.";

        var tipoComprobanteArca = (int)factura.TipoComprobante;

        var auth = await ObtenerAuthAsync(
            factura.EmpresaId,
            ambiente,
            "wsfe");

        var ultimo = await ObtenerUltimoNumeroAsync(
            auth,
            ambiente,
            puntoVenta.Numero,
            tipoComprobanteArca);

        var proximoNumero = ultimo + 1;

        var fechaComprobante = factura.FechaEmision.ToString("yyyyMMdd");
        var fechaServicioDesde = new DateTime(factura.PeriodoAnio, factura.PeriodoMes, 1)
            .ToString("yyyyMMdd");

        var fechaServicioHasta = new DateTime(factura.PeriodoAnio, factura.PeriodoMes, 1)
            .AddMonths(1)
            .AddDays(-1)
            .ToString("yyyyMMdd");

        var fechaVencimientoPago = factura.FechaVencimiento.ToString("yyyyMMdd");

        decimal total = factura.Total;

        decimal neto;
        decimal iva;

        if (factura.ImporteIva > 0)
        {
            neto = factura.Subtotal;
            iva = factura.ImporteIva;
        }
        else
        {
            neto = Math.Round(total / 1.21m, 2);
            iva = total - neto;
        }

        var totalStr = FormatoDecimal(total);
        var netoStr = FormatoDecimal(neto);
        var ivaStr = FormatoDecimal(iva);

        var soap = $"""
    <?xml version="1.0" encoding="utf-8"?>
    <soap:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                   xmlns:xsd="http://www.w3.org/2001/XMLSchema"
                   xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
      <soap:Body>
        <FECAESolicitar xmlns="http://ar.gov.afip.dif.FEV1/">
          <Auth>
            <Token>{auth.Token}</Token>
            <Sign>{auth.Sign}</Sign>
            <Cuit>{auth.Cuit}</Cuit>
          </Auth>
          <FeCAEReq>
            <FeCabReq>
              <CantReg>1</CantReg>
              <PtoVta>{puntoVenta.Numero}</PtoVta>
              <CbteTipo>{tipoComprobanteArca}</CbteTipo>
            </FeCabReq>
            <FeDetReq>
              <FECAEDetRequest>
                <Concepto>2</Concepto>
                <DocTipo>99</DocTipo>
                <DocNro>0</DocNro>
                <CbteDesde>{proximoNumero}</CbteDesde>
                <CbteHasta>{proximoNumero}</CbteHasta>
                <CbteFch>{fechaComprobante}</CbteFch>
                <ImpTotal>{totalStr}</ImpTotal>
                <ImpTotConc>0.00</ImpTotConc>
                <ImpNeto>{netoStr}</ImpNeto>
                <ImpOpEx>0.00</ImpOpEx>
                <ImpTrib>0.00</ImpTrib>
                <ImpIVA>{ivaStr}</ImpIVA>
                <FchServDesde>{fechaServicioDesde}</FchServDesde>
                <FchServHasta>{fechaServicioHasta}</FchServHasta>
                <FchVtoPago>{fechaVencimientoPago}</FchVtoPago>
                <MonId>PES</MonId>
                <MonCotiz>1.00</MonCotiz>
                <CondicionIVAReceptorId>{condicionIvaConsumidorFinal}</CondicionIVAReceptorId>
                <Iva>
                  <AlicIva>
                    <Id>5</Id>
                    <BaseImp>{netoStr}</BaseImp>
                    <Importe>{ivaStr}</Importe>
                  </AlicIva>
                </Iva>
              </FECAEDetRequest>
            </FeDetReq>
          </FeCAEReq>
        </FECAESolicitar>
      </soap:Body>
    </soap:Envelope>
    """;

        var respuesta = await LlamarWsfeAsync(
            soap,
            "http://ar.gov.afip.dif.FEV1/FECAESolicitar",
            ambiente);

        var xml = XDocument.Parse(respuesta);

        var errores = LeerErrores(xml);
        var observaciones = LeerObservaciones(xml);

        var resultado = xml
            .Descendants()
            .FirstOrDefault(x => x.Name.LocalName == "Resultado")
            ?.Value;

        var cae = xml
            .Descendants()
            .FirstOrDefault(x => x.Name.LocalName == "CAE")
            ?.Value;

        var caeVtoTexto = xml
            .Descendants()
            .FirstOrDefault(x => x.Name.LocalName == "CAEFchVto")
            ?.Value;

        if (resultado != "A" || string.IsNullOrWhiteSpace(cae))
        {
            factura.EstadoFiscal = EstadoFiscalFactura.Rechazada;
            factura.UpdatedAt = DateTime.Now;

            await GuardarResultadoFacturaElectronicaAsync(
                factura: factura,
                resultado: resultado ?? "R",
                cae: null,
                fechaVencimientoCae: null,
                observaciones: observaciones,
                errores: errores,
                requestResumen: $"PV {puntoVenta.Numero} - Tipo {tipoComprobanteArca} - Nro {proximoNumero}",
                responseResumen: respuesta);

            await _dbContext.SaveChangesAsync();

            return
                "ARCA rechazó la factura." + Environment.NewLine +
                $"Resultado: {resultado}" + Environment.NewLine +
                errores + Environment.NewLine +
                observaciones;
        }

        var fechaVencimientoCae = DateTime.ParseExact(
            caeVtoTexto!,
            "yyyyMMdd",
            CultureInfo.InvariantCulture);

        factura.PuntoVentaId = puntoVenta.Id;
        factura.PuntoVentaNumeroSnapshot = puntoVenta.Numero;
        factura.NumeroComprobante = proximoNumero;
        factura.NumeroComprobanteFormateado = $"{puntoVenta.Numero:0000}-{proximoNumero:00000000}";
        factura.EstadoFiscal = EstadoFiscalFactura.Autorizada;
        factura.UpdatedAt = DateTime.Now;

        await GuardarResultadoFacturaElectronicaAsync(
            factura: factura,
            resultado: resultado ?? "A",
            cae: cae,
            fechaVencimientoCae: fechaVencimientoCae,
            observaciones: observaciones,
            errores: errores,
            requestResumen: $"PV {puntoVenta.Numero} - Tipo {tipoComprobanteArca} - Nro {proximoNumero}",
            responseResumen: respuesta);

        await _dbContext.SaveChangesAsync();

        return
            "Factura autorizada correctamente en ARCA HOMOLOGACIÓN." + Environment.NewLine +
            $"Factura Id: {factura.Id}" + Environment.NewLine +
            $"Comprobante: {factura.NumeroComprobanteFormateado}" + Environment.NewLine +
            $"Tipo: {factura.TipoComprobante}" + Environment.NewLine +
            $"Total: {factura.Total:C2}" + Environment.NewLine +
            $"CAE: {cae}" + Environment.NewLine +
            $"Vencimiento CAE: {fechaVencimientoCae:dd/MM/yyyy}" + Environment.NewLine +
            observaciones;
    }

    private async Task<long> ObtenerUltimoNumeroAsync(
        ArcaAuthData auth,
        AmbienteFiscal ambiente,
        int puntoVenta,
        int tipoComprobante)
    {
        var soap = $"""
        <?xml version="1.0" encoding="utf-8"?>
        <soap:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                       xmlns:xsd="http://www.w3.org/2001/XMLSchema"
                       xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
          <soap:Body>
            <FECompUltimoAutorizado xmlns="http://ar.gov.afip.dif.FEV1/">
              <Auth>
                <Token>{auth.Token}</Token>
                <Sign>{auth.Sign}</Sign>
                <Cuit>{auth.Cuit}</Cuit>
              </Auth>
              <PtoVta>{puntoVenta}</PtoVta>
              <CbteTipo>{tipoComprobante}</CbteTipo>
            </FECompUltimoAutorizado>
          </soap:Body>
        </soap:Envelope>
        """;

        var respuesta = await LlamarWsfeAsync(
            soap,
            "http://ar.gov.afip.dif.FEV1/FECompUltimoAutorizado",
            ambiente);

        var xml = XDocument.Parse(respuesta);

        var errores = LeerErrores(xml);

        if (!string.IsNullOrWhiteSpace(errores))
            throw new Exception(errores);

        var cbteNroTexto = xml
            .Descendants()
            .FirstOrDefault(x => x.Name.LocalName == "CbteNro")
            ?.Value;

        if (long.TryParse(cbteNroTexto, out var numero))
            return numero;

        return 0;
    }

    private async Task<ArcaAuthData> ObtenerAuthAsync(
        int empresaId,
        AmbienteFiscal ambiente,
        string servicio)
    {
        var empresa = await _dbContext.Empresas
            .AsNoTracking()
            .FirstAsync(x => x.Id == empresaId);

        var token = await _dbContext.AfipTokens
            .AsNoTracking()
            .Where(x =>
                x.EmpresaId == empresaId &&
                x.Ambiente == ambiente &&
                x.Servicio == servicio &&
                x.FechaHasta > DateTime.Now.AddMinutes(5))
            .OrderByDescending(x => x.FechaHasta)
            .FirstOrDefaultAsync();

        if (token is null)
            throw new Exception("No hay Token/Sign vigente. Primero ejecutá Probar WSAA homologación.");

        return new ArcaAuthData
        {
            Cuit = long.Parse(empresa.Cuit),
            Token = token.Token,
            Sign = token.Sign
        };
    }

    private async Task<string> LlamarWsfeAsync(
        string soap,
        string soapAction,
        AmbienteFiscal ambiente)
    {
        var url = ambiente == AmbienteFiscal.Homologacion
            ? "https://wswhomo.afip.gov.ar/wsfev1/service.asmx"
            : "https://servicios1.afip.gov.ar/wsfev1/service.asmx";

        var client = _httpClientFactory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = new StringContent(soap, Encoding.UTF8, "text/xml");
        request.Headers.Add("SOAPAction", soapAction);

        using var response = await client.SendAsync(request);
        var responseText = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"WSFE respondió HTTP {(int)response.StatusCode}. Respuesta: {responseText}");
        }

        return responseText;
    }

    private static string LeerErrores(XDocument xml)
    {
        var errores = xml
            .Descendants()
            .Where(x => x.Name.LocalName == "Err")
            .Select(x =>
            {
                var codigo = x.Elements().FirstOrDefault(e => e.Name.LocalName == "Code")?.Value;
                var mensaje = x.Elements().FirstOrDefault(e => e.Name.LocalName == "Msg")?.Value;

                return $"Código {codigo}: {mensaje}";
            })
            .ToList();

        return string.Join(Environment.NewLine, errores);
    }

    private static string LeerObservaciones(XDocument xml)
    {
        var observaciones = xml
            .Descendants()
            .Where(x => x.Name.LocalName == "Obs")
            .Select(x =>
            {
                var codigo = x.Elements().FirstOrDefault(e => e.Name.LocalName == "Code")?.Value;
                var mensaje = x.Elements().FirstOrDefault(e => e.Name.LocalName == "Msg")?.Value;

                return $"Observación {codigo}: {mensaje}";
            })
            .ToList();

        return string.Join(Environment.NewLine, observaciones);
    }

    private static string FormatoDecimal(decimal valor)
    {
        return valor.ToString("0.00", CultureInfo.InvariantCulture);
    }

    private class ArcaAuthData
    {
        public long Cuit { get; set; }
        public string Token { get; set; } = string.Empty;
        public string Sign { get; set; } = string.Empty;
    }
}