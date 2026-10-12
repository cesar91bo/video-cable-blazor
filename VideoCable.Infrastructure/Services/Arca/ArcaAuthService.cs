using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using VideoCable.Domain.Entities;
using VideoCable.Domain.Enums;
using VideoCable.Infrastructure.Data;

namespace VideoCable.Infrastructure.Services.Arca;

public class ArcaAuthService
{
    private readonly IDbContextFactory<AppDbContext> _factory;
    private readonly ArcaSecretos _secretos;
    private readonly IHttpClientFactory _httpClientFactory;

    public ArcaAuthService(
        IDbContextFactory<AppDbContext> factory,
        IHttpClientFactory httpClientFactory,
        ArcaSecretos secretos)
    {
        _factory = factory;
        _secretos = secretos;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string> ObtenerTokenSignAsync(int empresaId, AmbienteFiscal ambiente, string servicio = "wsfe")
    {
        var datos = await ObtenerCredencialesAsync(empresaId, ambiente, servicio);
        return $"WSAA disponible para {ambiente}. Token válido hasta {datos.FechaHasta:dd/MM/yyyy HH:mm}.";
    }

    public async Task<CredencialesArca> ObtenerCredencialesAsync(int empresaId, AmbienteFiscal ambiente, string servicio = "wsfe")
    {
        if (!Enum.IsDefined(ambiente)) throw new InvalidOperationException("Ambiente ARCA no válido.");
        await using var db = await _factory.CreateDbContextAsync();
        await using var bloqueo = await ArcaSqlLock.Adquirir(db, $"ARCA-AUTH:{empresaId}:{ambiente}:{servicio}");
        var empresa = await db.Empresas.AsNoTracking().SingleAsync(x => x.Id == empresaId);
        if (!ArcaFiscalRules.CuitValido(empresa.Cuit)) throw new InvalidOperationException("CUIT de empresa no válido.");
        var tokenVigente = await db.AfipTokens
            .AsNoTracking()
            .Where(x =>
                x.EmpresaId == empresaId &&
                x.Ambiente == ambiente &&
                x.Servicio == servicio &&
                x.FechaHasta > DateTime.Now.AddMinutes(5))
            .OrderByDescending(x => x.FechaHasta)
            .FirstOrDefaultAsync();

        if (tokenVigente is not null)
        {
            return new CredencialesArca(long.Parse(ArcaFiscalRules.SoloNumeros(empresa.Cuit)),
                _secretos.Leer(tokenVigente.Token), _secretos.Leer(tokenVigente.Sign), tokenVigente.FechaHasta);
        }

        var certificadoEmpresa = await db.CertificadosEmpresa
            .AsNoTracking()
            .Where(x =>
                x.EmpresaId == empresaId &&
                x.Ambiente == ambiente &&
                x.Activo &&
                !x.IsDeleted)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync();

        if (certificadoEmpresa is null)
            throw new InvalidOperationException($"Cargue un certificado activo de {ambiente} en Configuración ARCA.");

        using var certificado = _secretos.Abrir(certificadoEmpresa);

        var traXml = CrearLoginTicketRequest(servicio);

        var cmsFirmadoBase64 = FirmarLoginTicketRequest(traXml, certificado);

        var respuestaWsaa = await LlamarWsaaAsync(cmsFirmadoBase64, ambiente);

        var datos = LeerRespuestaWsaa(respuestaWsaa);

        var nuevoToken = new AfipToken
        {
            EmpresaId = empresaId,
            Ambiente = ambiente,
            Servicio = servicio,
            Token = _secretos.Proteger(datos.Token),
            Sign = _secretos.Proteger(datos.Sign),
            FechaDesde = datos.FechaDesde,
            FechaHasta = datos.FechaHasta,
            CreatedAt = DateTime.Now
        };

        db.AfipTokens.Add(nuevoToken);
        await db.SaveChangesAsync();

        return new CredencialesArca(long.Parse(ArcaFiscalRules.SoloNumeros(empresa.Cuit)), datos.Token, datos.Sign, datos.FechaHasta);
    }

    private static string CrearLoginTicketRequest(string servicio)
    {
        var ahora = DateTime.Now;

        var xml = new XDocument(
            new XElement("loginTicketRequest",
                new XAttribute("version", "1.0"),
                new XElement("header",
                    new XElement("uniqueId", DateTimeOffset.Now.ToUnixTimeSeconds()),
                    new XElement("generationTime", ahora.AddMinutes(-10).ToString("yyyy-MM-ddTHH:mm:sszzz")),
                    new XElement("expirationTime", ahora.AddHours(12).ToString("yyyy-MM-ddTHH:mm:sszzz"))
                ),
                new XElement("service", servicio)
            )
        );

        return xml.ToString(SaveOptions.DisableFormatting);
    }

    private static string FirmarLoginTicketRequest(string traXml, X509Certificate2 certificado)
    {
        var bytes = Encoding.UTF8.GetBytes(traXml);

        var contentInfo = new ContentInfo(bytes);
        var signedCms = new SignedCms(contentInfo, detached: false);

        var cmsSigner = new CmsSigner(SubjectIdentifierType.IssuerAndSerialNumber, certificado)
        {
            IncludeOption = X509IncludeOption.EndCertOnly
        };

        signedCms.ComputeSignature(cmsSigner);

        return Convert.ToBase64String(signedCms.Encode());
    }

    private async Task<string> LlamarWsaaAsync(string cmsFirmadoBase64, AmbienteFiscal ambiente)
    {
        var url = ambiente == AmbienteFiscal.Homologacion
            ? "https://wsaahomo.afip.gov.ar/ws/services/LoginCms"
            : "https://wsaa.afip.gov.ar/ws/services/LoginCms";

        var soap = $"""
        <?xml version="1.0" encoding="UTF-8"?>
        <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:wsaa="http://wsaa.view.sua.dvadac.desein.afip.gov">
            <soapenv:Header/>
            <soapenv:Body>
                <wsaa:loginCms>
                    <wsaa:in0>{cmsFirmadoBase64}</wsaa:in0>
                </wsaa:loginCms>
            </soapenv:Body>
        </soapenv:Envelope>
        """;

        var client = _httpClientFactory.CreateClient("Arca");

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = new StringContent(soap, Encoding.UTF8, "text/xml");
        request.Headers.Add("SOAPAction", "");

        using var response = await client.SendAsync(request);
        var responseText = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"WSAA respondió HTTP {(int)response.StatusCode}. Revise certificado, relación del servicio y ambiente.");
        }

        return responseText;
    }

    private static WsaaLoginResult LeerRespuestaWsaa(string soapResponse)
    {
        var xml = XDocument.Parse(soapResponse);

        var loginCmsReturn = xml
            .Descendants()
            .FirstOrDefault(x => x.Name.LocalName == "loginCmsReturn")
            ?.Value;

        if (string.IsNullOrWhiteSpace(loginCmsReturn))
            throw new Exception("WSAA no devolvió un ticket de acceso. Revise la relación del certificado con wsfe.");

        var ticketXml = XDocument.Parse(loginCmsReturn);

        var token = ticketXml
            .Descendants()
            .First(x => x.Name.LocalName == "token")
            .Value;

        var sign = ticketXml
            .Descendants()
            .First(x => x.Name.LocalName == "sign")
            .Value;

        var generationTimeTexto = ticketXml
            .Descendants()
            .First(x => x.Name.LocalName == "generationTime")
            .Value;

        var expirationTimeTexto = ticketXml
            .Descendants()
            .First(x => x.Name.LocalName == "expirationTime")
            .Value;

        return new WsaaLoginResult
        {
            Token = token,
            Sign = sign,
            FechaDesde = DateTime.Parse(generationTimeTexto),
            FechaHasta = DateTime.Parse(expirationTimeTexto)
        };
    }

    private class WsaaLoginResult
    {
        public string Token { get; set; } = string.Empty;
        public string Sign { get; set; } = string.Empty;
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
    }
}

public sealed record CredencialesArca(long Cuit, string Token, string Sign, DateTime FechaHasta);
