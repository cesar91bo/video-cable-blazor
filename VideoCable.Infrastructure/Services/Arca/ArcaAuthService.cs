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
    private readonly AppDbContext _dbContext;
    private readonly IHttpClientFactory _httpClientFactory;

    public ArcaAuthService(
        AppDbContext dbContext,
        IHttpClientFactory httpClientFactory)
    {
        _dbContext = dbContext;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string> ObtenerTokenSignAsync(int empresaId, AmbienteFiscal ambiente, string servicio = "wsfe")
    {
        var tokenVigente = await _dbContext.AfipTokens
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
            return
                "Token vigente reutilizado correctamente." + Environment.NewLine +
                $"Servicio: {tokenVigente.Servicio}" + Environment.NewLine +
                $"Válido desde: {tokenVigente.FechaDesde:dd/MM/yyyy HH:mm}" + Environment.NewLine +
                $"Válido hasta: {tokenVigente.FechaHasta:dd/MM/yyyy HH:mm}" + Environment.NewLine +
                $"Token largo: {tokenVigente.Token.Length} caracteres" + Environment.NewLine +
                $"Sign largo: {tokenVigente.Sign.Length} caracteres";
        }

        var certificadoEmpresa = await _dbContext.CertificadosEmpresa
            .AsNoTracking()
            .Where(x =>
                x.EmpresaId == empresaId &&
                x.Ambiente == ambiente &&
                x.Activo &&
                !x.IsDeleted)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync();

        if (certificadoEmpresa is null)
            return "No se encontró certificado activo para la empresa y ambiente.";

        if (string.IsNullOrWhiteSpace(certificadoEmpresa.RutaArchivo))
            return "El certificado no tiene ruta configurada.";

        if (!File.Exists(certificadoEmpresa.RutaArchivo))
            return $"No existe el archivo del certificado: {certificadoEmpresa.RutaArchivo}";

        var certificado = new X509Certificate2(
            certificadoEmpresa.RutaArchivo,
            certificadoEmpresa.Password,
            X509KeyStorageFlags.MachineKeySet |
            X509KeyStorageFlags.PersistKeySet |
            X509KeyStorageFlags.Exportable);

        if (!certificado.HasPrivateKey)
            return "El certificado fue cargado, pero no tiene clave privada.";

        var traXml = CrearLoginTicketRequest(servicio);

        var cmsFirmadoBase64 = FirmarLoginTicketRequest(traXml, certificado);

        var respuestaWsaa = await LlamarWsaaAsync(cmsFirmadoBase64, ambiente);

        var datos = LeerRespuestaWsaa(respuestaWsaa);

        var nuevoToken = new AfipToken
        {
            EmpresaId = empresaId,
            Ambiente = ambiente,
            Servicio = servicio,
            Token = datos.Token,
            Sign = datos.Sign,
            FechaDesde = datos.FechaDesde,
            FechaHasta = datos.FechaHasta,
            CreatedAt = DateTime.Now
        };

        _dbContext.AfipTokens.Add(nuevoToken);
        await _dbContext.SaveChangesAsync();

        return
            "WSAA respondió correctamente." + Environment.NewLine +
            $"Servicio: {servicio}" + Environment.NewLine +
            $"Válido desde: {datos.FechaDesde:dd/MM/yyyy HH:mm}" + Environment.NewLine +
            $"Válido hasta: {datos.FechaHasta:dd/MM/yyyy HH:mm}" + Environment.NewLine +
            $"Token largo: {datos.Token.Length} caracteres" + Environment.NewLine +
            $"Sign largo: {datos.Sign.Length} caracteres";
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

        var client = _httpClientFactory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = new StringContent(soap, Encoding.UTF8, "text/xml");
        request.Headers.Add("SOAPAction", "");

        using var response = await client.SendAsync(request);
        var responseText = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"WSAA respondió HTTP {(int)response.StatusCode}. Respuesta: {responseText}");
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
            throw new Exception($"No se encontró loginCmsReturn en la respuesta WSAA. Respuesta: {soapResponse}");

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