using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using VideoCable.Domain.Entities;
using VideoCable.Domain.Enums;
using VideoCable.Infrastructure.Data;
using VideoCable.Infrastructure.Services.Arca;

var verificaciones = 0;
void Verificar(bool condicion, string mensaje) { if (!condicion) throw new Exception(mensaje); verificaciones++; }
void Rechazar(Action accion, string mensaje) { try { accion(); } catch (InvalidOperationException) { verificaciones++; return; } throw new Exception(mensaje); }
async Task RechazarAsync(Func<Task> accion, string mensaje) { try { await accion(); } catch (InvalidOperationException) { verificaciones++; return; } throw new Exception(mensaje); }
Factura Crear(int periodo = 1) => new()
{
    Empresa = new Empresa { Cuit = "30712345671", RazonSocial = "Pruebas", Direccion = "Pruebas", CondicionIva = CondicionIvaEmpresa.ResponsableInscripto },
    Cliente = new Cliente { Nombre = "Prueba", Codigo = 1, Direccion = "Prueba", TipoDocumento = TipoDocumentoCliente.DNI, NumeroDocumento = "12345678", RegimenImpositivo = RegimenImpositivo.ConsumidorFinal },
    PeriodoMes = periodo, PeriodoAnio = DateTime.Today.Year, FechaEmision = DateTime.Today, FechaVencimiento = DateTime.Today.AddDays(10), Total = 1210m,
    Items = [new FacturaItem { Descripcion = "Internet", Cantidad = 1, Total = 1210m }]
};
Verificar(ArcaFiscalRules.CuitValido("30-71234567-1"), "CUIT con formato válido.");
Verificar(!ArcaFiscalRules.CuitValido("30712345670"), "Debe rechazar CUIT con dígito inválido.");
var f = Crear(); ArcaFiscalRules.Preparar(f, DateTime.Today);
Verificar(f.TipoComprobante == TipoComprobante.FacturaB && f.Subtotal == 1000m && f.ImporteIva == 210m && f.Items.Single().Subtotal == 1000m, "IVA impreso y enviado deben coincidir.");
Verificar(f.DocTipoReceptorArca == 96 && f.DocNroReceptorArca == 12345678 && f.CondicionIvaReceptorArca == 5, "Consumidor final con DNI real.");
var xml = ArcaWsfeClient.Solicitud(f, 1, 1);
Verificar(ArcaWsfeClient.Valor(xml, "ImpIVA") == "210.00" && ArcaWsfeClient.Valor(xml, "DocTipo") == "96", "SOAP con valores fiscales correctos.");
var c = Crear(); c.Empresa.CondicionIva = CondicionIvaEmpresa.Monotributista; ArcaFiscalRules.Preparar(c, DateTime.Today);
Verificar(c.TipoComprobante == TipoComprobante.FacturaC && c.ImporteIva == 0 && c.Subtotal == c.Total && !ArcaWsfeClient.Detalle(c, 1).Descendants().Any(x => x.Name.LocalName == "Iva"), "Factura C sin grupo IVA.");
var a = Crear(); a.Cliente.RegimenImpositivo = RegimenImpositivo.Monotributista; a.Cliente.Cuit = "20123456786"; ArcaFiscalRules.Preparar(a, DateTime.Today);
Verificar(a.TipoComprobante == TipoComprobante.FacturaA && a.DocTipoReceptorArca == 80 && a.CondicionIvaReceptorArca == 6, "Factura A para monotributista con CUIT.");
var sinDoc = Crear(); sinDoc.Cliente.NumeroDocumento = null; ArcaFiscalRules.Preparar(sinDoc, DateTime.Today);
Verificar(sinDoc.TipoComprobante == TipoComprobante.FacturaB && sinDoc.DocTipoReceptorArca == 99 && sinDoc.DocNroReceptorArca == 0 && sinDoc.ClienteDocumentoSnapshot is null, "Factura B a consumidor final sin documento, sin imprimir documento cero.");
Verificar(ArcaWsfeClient.Valor(ArcaWsfeClient.Solicitud(sinDoc, 3, 1), "DocNro") == "0", "SOAP de consumidor final sin identificar con documento cero.");
var docCero = Crear(); docCero.Cliente.NumeroDocumento = "0"; ArcaFiscalRules.Preparar(docCero, DateTime.Today);
Verificar(docCero.DocTipoReceptorArca == 99, "Documento cero equivale a consumidor final sin identificar.");
var sinDocC = Crear(); sinDocC.Cliente.NumeroDocumento = null; sinDocC.Empresa.CondicionIva = CondicionIvaEmpresa.Monotributista; ArcaFiscalRules.Preparar(sinDocC, DateTime.Today);
Verificar(sinDocC.TipoComprobante == TipoComprobante.FacturaC && sinDocC.DocTipoReceptorArca == 99, "Factura C sin documento bajo el umbral.");
var montoLimite = Crear(); montoLimite.Cliente.NumeroDocumento = null; montoLimite.Total = ArcaFiscalRules.UmbralIdentificacionConsumidorFinal; montoLimite.Items.Single().Total = montoLimite.Total;
Rechazar(() => ArcaFiscalRules.Preparar(montoLimite, DateTime.Today), "Desde el umbral debe identificar al consumidor final.");
montoLimite.Total -= 0.01m; montoLimite.Items.Single().Total = montoLimite.Total; ArcaFiscalRules.Preparar(montoLimite, DateTime.Today); Verificar(montoLimite.DocTipoReceptorArca == 99, "Debajo del umbral no debe exigir documento.");
var sinCuitA = Crear(); sinCuitA.Cliente.RegimenImpositivo = RegimenImpositivo.ResponsableInscripto; sinCuitA.Cliente.NumeroDocumento = null;
Rechazar(() => ArcaFiscalRules.Preparar(sinCuitA, DateTime.Today), "Factura A sigue requiriendo CUIT.");
var docInvalido = Crear(); docInvalido.Cliente.NumeroDocumento = "123456789";
Rechazar(() => ArcaFiscalRules.Preparar(docInvalido, DateTime.Today), "Documento informado inválido no se omite.");
var antigua = Crear(); antigua.FechaEmision = DateTime.Today.AddDays(-11); Rechazar(() => ArcaFiscalRules.ValidarFecha(antigua, DateTime.Today), "Debe rechazar fecha de servicio fuera de rango.");
var cero = Crear(); cero.Empresa.AlicuotaIvaArca = 0; ArcaFiscalRules.Preparar(cero, DateTime.Today); Verificar(cero.ImporteIva == 0 && ArcaFiscalRules.CodigoIva(0) == 3, "IVA cero configurable.");
f.NumeroComprobante = 1; f.PuntoVentaNumeroSnapshot = 1;
var url = ArcaQr.CrearUrl(f, "12345678901234");
using var json = JsonDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(Uri.UnescapeDataString(url.Split("?p=")[1]))));
Verificar(json.RootElement.GetProperty("importe").GetDecimal() == 1210 && json.RootElement.GetProperty("tipoDocRec").GetInt32() == 96 && json.RootElement.GetProperty("codAut").GetInt64() == 12345678901234, "QR con importe, documento y CAE numéricos.");
var png = Convert.FromBase64String(ArcaQr.Imagen(url).Split(',')[1]); Verificar(png.Take(8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }), "QR PNG válido.");
var opciones = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=NoUtilizada;Integrated Security=true;TrustServerCertificate=true").Options;
var factory = new Factory(opciones);
var provider = new EphemeralDataProtectionProvider(); var secretos = new ArcaSecretos(provider, factory);
var protegido = secretos.Proteger("clave-de-prueba"); Verificar(!protegido.Contains("clave-de-prueba") && secretos.Leer(protegido) == "clave-de-prueba", "Contraseña cifrada y recuperable.");
var simulador = new Simulador(); var http = new HttpFactory(simulador); var wsfe = new ArcaWsfeClient(http); var cred = new CredencialesArca(30712345671, "token-prueba", "sign-prueba", DateTime.Now.AddHours(1));
var puntoPrueba = XDocument.Parse("<ResultGet><PtoVenta><Nro>00003</Nro><EmisionTipo> CAE </EmisionTipo><Bloqueado>N</Bloqueado><FchBaja>NULL</FchBaja></PtoVenta></ResultGet>");
ArcaWsfeClient.ValidarPuntoVenta(puntoPrueba, cred.Cuit, AmbienteFiscal.Produccion, 3); verificaciones++;
puntoPrueba.Descendants("EmisionTipo").Single().Value = "CAE - Ri Iva";
ArcaWsfeClient.ValidarPuntoVenta(puntoPrueba, cred.Cuit, AmbienteFiscal.Produccion, 3); verificaciones++;
void DiagnosticoPunto(int numero, string esperado)
{
    try { ArcaWsfeClient.ValidarPuntoVenta(puntoPrueba, cred.Cuit, AmbienteFiscal.Produccion, numero); }
    catch (InvalidOperationException ex) { Verificar(ex.Message.Contains(esperado) && ex.Message.Contains(cred.Cuit.ToString()), "Diagnóstico de punto de venta con causa y CUIT."); return; }
    throw new Exception("Debía rechazar el punto de venta.");
}
DiagnosticoPunto(4, "Puntos devueltos: 00003");
puntoPrueba.Descendants("Bloqueado").Single().Value = "S"; DiagnosticoPunto(3, "bloqueado: 'S'");
puntoPrueba.Descendants("Bloqueado").Single().Value = "N";
puntoPrueba.Descendants("FchBaja").Single().Value = "20261011"; DiagnosticoPunto(3, "fecha de baja: '20261011'");
puntoPrueba.Descendants("FchBaja").Single().Value = "";
puntoPrueba.Descendants("EmisionTipo").Single().Value = "CAEA"; DiagnosticoPunto(3, "Tipo de emisión: 'CAEA'");
puntoPrueba.Descendants("EmisionTipo").Single().Value = "CAEA - Ri Iva"; DiagnosticoPunto(3, "Tipo de emisión: 'CAEA - Ri Iva'");
puntoPrueba.Descendants("EmisionTipo").Single().Value = "DESCONOCIDO"; DiagnosticoPunto(3, "Tipo de emisión: 'DESCONOCIDO'");
Verificar(await wsfe.Ultimo(cred, AmbienteFiscal.Homologacion, 1, 6) == 0, "Punto sin comprobantes inicia en cero.");
Verificar(await wsfe.Consultar(cred, AmbienteFiscal.Homologacion, 1, 6, 1) is null, "Solo ausencia 602 permite reenviar.");
simulador.UltimoInvalido = true; await RechazarAsync(async () => await wsfe.Ultimo(cred, AmbienteFiscal.Homologacion, 1, 6), "No debe convertir una respuesta inválida en número cero."); simulador.UltimoInvalido = false;
await wsfe.Emitir(cred, AmbienteFiscal.Homologacion, f, 1, 1); var consulta = await wsfe.Consultar(cred, AmbienteFiscal.Homologacion, 1, 6, 1);
ArcaFacturaService.ValidarCoincidencia(f, 1, 1, consulta!); verificaciones++;
consulta!.Elements().First(x => x.Name.LocalName == "ImpTotal").Value = "999.00"; Rechazar(() => ArcaFacturaService.ValidarCoincidencia(f, 1, 1, consulta), "No debe recuperar un comprobante distinto.");

if (args.Contains("--sql"))
{
    var nombre = "VideoCableArcaChecks_" + Guid.NewGuid().ToString("N");
    var directorio = Path.Combine(Path.GetTempPath(), nombre); Directory.CreateDirectory(directorio);
    var master = "Server=(localdb)\\MSSQLLocalDB;Database=master;Integrated Security=true;TrustServerCertificate=true;Connect Timeout=15";
    await using var conexion = new SqlConnection(master); await conexion.OpenAsync();
    await using (var crear = conexion.CreateCommand())
    {
        crear.CommandText = $"CREATE DATABASE [{nombre}] ON PRIMARY (NAME=N'{nombre}', FILENAME=N'{Path.Combine(directorio, nombre + ".mdf").Replace("'", "''")}') LOG ON (NAME=N'{nombre}_log', FILENAME=N'{Path.Combine(directorio, nombre + ".ldf").Replace("'", "''")}');";
        await crear.ExecuteNonQueryAsync();
    }
    var builder = new SqlConnectionStringBuilder(master) { InitialCatalog = nombre };
    var testFactory = new Factory(new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(builder.ConnectionString).Options);
    var testSecretos = new ArcaSecretos(new EphemeralDataProtectionProvider(), testFactory);
    var server = new Simulador(); var transport = new HttpFactory(server);
    var auth = new ArcaAuthService(testFactory, transport, testSecretos);
    var servicio = new ArcaFacturaService(testFactory, auth, new ArcaWsfeClient(transport));
    try
    {
        await using (var db = testFactory.CreateDbContext()) { await db.Database.MigrateAsync(); }
        var empresa = Crear().Empresa; var cliente = Crear().Cliente;
        var plan = new ServicioPlan { Descripcion = "Internet", TipoServicio = TipoServicio.Internet, PrecioActual = 1210 };
        var caja = new CajaDistribucion { Descripcion = "Caja de prueba" };
        var suscripcion = new Suscripcion { Cliente = cliente, ServicioPlan = plan, CajaDistribucion = caja, FechaAlta = DateTime.Today };
        await using (var db = testFactory.CreateDbContext()) { db.Empresas.Add(empresa); db.Suscripciones.Add(suscripcion); await db.SaveChangesAsync(); db.PuntosVenta.Add(new PuntoVenta { EmpresaId = empresa.Id, Numero = 1, Descripcion = "Prueba", Ambiente = AmbienteFiscal.Homologacion, Tipo = TipoPuntoVenta.Electronico, Predeterminado = true }); await db.SaveChangesAsync(); }
        using var rsa = RSA.Create(2048); var solicitud = new CertificateRequest("CN=Prueba local", rsa, System.Security.Cryptography.HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var certificado = solicitud.CreateSelfSigned(DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now.AddDays(1));
        await testSecretos.GuardarCertificadoAsync(empresa.Id, AmbienteFiscal.Homologacion, certificado.Export(X509ContentType.Pfx, "prueba"), "prueba");
        async Task<int> Nueva(int mes) { var factura = Crear(mes); factura.Empresa = null!; factura.Cliente = null!; factura.EmpresaId = empresa.Id; factura.ClienteId = cliente.Id; factura.SuscripcionId = suscripcion.Id; factura.TipoComprobante = TipoComprobante.FacturaX; await using var db = testFactory.CreateDbContext(); db.Facturas.Add(factura); await db.SaveChangesAsync(); await servicio.PrepararFacturaAsync(factura.Id); return factura.Id; }
        await servicio.ProbarConfiguracionAsync(empresa.Id, AmbienteFiscal.Homologacion); Verificar(server.Envios == 0, "La prueba de conexión no debe emitir.");
        var uno = await Nueva(1); var dos = await Nueva(2);
        await Task.WhenAll(servicio.AutorizarFacturaAsync(uno, AmbienteFiscal.Homologacion), servicio.AutorizarFacturaAsync(dos, AmbienteFiscal.Homologacion));
        Verificar(server.Envios == 2, "Dos usuarios deben emitir solo dos comprobantes.");
        await servicio.AutorizarFacturaAsync(uno, AmbienteFiscal.Homologacion); Verificar(server.Envios == 2, "Repetir una autorizada no debe emitir.");
        var tres = await Nueva(3); server.CortarDespuesAutorizar = true;
        await RechazarAsync(async () => await servicio.AutorizarFacturaAsync(tres, AmbienteFiscal.Homologacion), "El corte debe dejar pendiente recuperación.");
        var cuatro = await Nueva(4);
        await RechazarAsync(async () => await servicio.AutorizarFacturaAsync(cuatro, AmbienteFiscal.Homologacion), "No debe emitir mientras existe una anterior incierta.");
        var recuperada = await servicio.AutorizarFacturaAsync(tres, AmbienteFiscal.Homologacion);
        Verificar(recuperada.Contains("recuperada") && server.Envios == 3, "Recuperar después del corte no debe duplicar la emisión.");
        await using (var db = testFactory.CreateDbContext()) { var e = await db.Empresas.SingleAsync(); e.LimiteFacturacionMensualArca = 4000m; await db.SaveChangesAsync(); }
        try { await servicio.AutorizarFacturaAsync(cuatro, AmbienteFiscal.Homologacion); throw new Exception("Faltó aviso de límite."); } catch (AvisoLimiteFacturacionException) { verificaciones++; }
        Verificar(server.Envios == 3, "El aviso debe ocurrir antes del envío.");
        await servicio.AutorizarFacturaAsync(cuatro, AmbienteFiscal.Homologacion, true); Verificar(server.Envios == 4, "El usuario puede continuar después del aviso.");
        await RechazarAsync(async () => await servicio.AutorizarFacturaAsync(uno, AmbienteFiscal.Produccion), "Un CAE de homologación no debe reutilizarse en producción.");
        await using (var db = testFactory.CreateDbContext())
        {
            var facturas = await db.Facturas.ToListAsync(); var electronicas = await db.FacturasElectronicas.ToListAsync();
            Verificar(facturas.Select(x => x.NumeroComprobante).Distinct().Count() == 4, "Numeración única concurrente.");
            Verificar(electronicas.All(x => x.Resultado == "A" && x.QrUrl is not null && !x.RequestResumen!.Contains("token-prueba")), "CAE, QR y pedidos sin credenciales persistidos.");
            Verificar(server.Wsaa == 1, "WSAA debe renovarse una vez y reutilizar el token vigente.");
            var token = await db.AfipTokens.SingleAsync(); token.FechaHasta = DateTime.Now.AddMinutes(-1); await db.SaveChangesAsync();
        }
        await servicio.ProbarConfiguracionAsync(empresa.Id, AmbienteFiscal.Homologacion); Verificar(server.Wsaa == 2, "El token vencido debe renovarse automáticamente.");
        // Renderizar los componentes reales con la misma base aislada.
        var servicios = new ServiceCollection(); servicios.AddLogging();
        servicios.AddSingleton<IDbContextFactory<AppDbContext>>(testFactory);
        servicios.AddScoped(_ => testFactory.CreateDbContext());
        servicios.AddSingleton(testSecretos); servicios.AddSingleton(servicio);
        servicios.AddSingleton<IJSRuntime>(new JsPrueba()); servicios.AddSingleton<NavigationManager>(new NavegacionPrueba());
        await using var sp = servicios.BuildServiceProvider();
        await using var renderer = new RenderizadorPruebas(sp, sp.GetRequiredService<ILoggerFactory>());
        async Task<string> Renderizar(Type tipo, ParameterView parametros) => await renderer.Dispatcher.InvokeAsync(async () => { var resultado = renderer.BeginRenderingComponent(tipo, parametros); await resultado.QuiescenceTask; return resultado.ToHtmlString(); });
        var impreso = await Renderizar(typeof(VideoCable.Web.Components.Pages.FacturaImprimir), ParameterView.FromDictionary(new Dictionary<string, object?> { ["FacturaId"] = uno }));
        Verificar(impreso.Contains("data:image/png;base64,") && impreso.Contains("SIN VALIDEZ FISCAL"), "La impresión real debe incluir QR y marca de homologación.");
        var configuracion = await Renderizar(typeof(VideoCable.Web.Components.Pages.ConfiguracionArca), ParameterView.Empty);
        Verificar(WebUtility.HtmlDecode(configuracion).Contains("Probar conexión sin emitir") && !configuracion.Contains("token-prueba") && !configuracion.Contains("dp:v1:"), "La configuración debe renderizar sin exponer secretos.");
        await servicio.VerificarAutorizadaAsync(uno); Verificar(server.Envios == 4, "Verificar una autorizada no debe volver a emitir.");
        await using (var db = testFactory.CreateDbContext()) { var vieja = await db.Facturas.Include(x => x.Items).SingleAsync(x => x.Id == uno); vieja.DocTipoReceptorArca = null; vieja.Subtotal = vieja.Total; vieja.ImporteIva = 0; foreach (var item in vieja.Items) { item.Subtotal = item.Total; item.ImporteIva = 0; } await db.SaveChangesAsync(); }
        await servicio.VerificarAutorizadaAsync(uno);
        await using (var db = testFactory.CreateDbContext()) { var corregida = await db.Facturas.SingleAsync(x => x.Id == uno); Verificar(corregida.Subtotal == 1000 && corregida.ImporteIva == 210 && corregida.DocTipoReceptorArca == 96, "Debe conciliar el desglose antiguo con el comprobante oficial sin cambiar el total."); }
        // Un rechazo concluyente permite corregir y volver a intentar sin reserva incierta.
        var cinco = await Nueva(5); server.RechazarEnvio = true;
        var rechazo = await servicio.AutorizarFacturaAsync(cinco, AmbienteFiscal.Homologacion, true);
        Verificar(rechazo.Contains("rechazó"), "Debe guardar un rechazo concluyente.");
        await servicio.PrepararFacturaAsync(cinco); await servicio.AutorizarFacturaAsync(cinco, AmbienteFiscal.Homologacion, true);
        await using (var db = testFactory.CreateDbContext()) { Verificar((await db.FacturasElectronicas.SingleAsync(x => x.FacturaId == cinco)).Resultado == "A", "Debe permitir corregir una rechazada."); var e = await db.Empresas.SingleAsync(); e.AmbienteArca = AmbienteFiscal.Produccion; e.LimiteFacturacionMensualArca = null; await db.SaveChangesAsync(); db.PuntosVenta.Add(new PuntoVenta { EmpresaId = empresa.Id, Numero = 1, Descripcion = "Producción simulada", Ambiente = AmbienteFiscal.Produccion, Tipo = TipoPuntoVenta.Electronico, Predeterminado = true }); await db.SaveChangesAsync(); }
        await testSecretos.GuardarCertificadoAsync(empresa.Id, AmbienteFiscal.Produccion, certificado.Export(X509ContentType.Pfx, "prueba"), "prueba");
        var seis = await Nueva(6); await servicio.AutorizarFacturaAsync(seis, AmbienteFiscal.Produccion);
        await using (var db = testFactory.CreateDbContext()) { var real = await db.Facturas.SingleAsync(x => x.Id == seis); Verificar(real.AmbienteArca == AmbienteFiscal.Produccion && real.NumeroComprobante == 1, "Producción debe usar credenciales y numeración independientes."); var e = await db.Empresas.SingleAsync(); e.AlicuotaIvaArca = 0; await db.SaveChangesAsync(); }
        var siete = await Nueva(7); await using (var db = testFactory.CreateDbContext()) Verificar((await db.Facturas.SingleAsync(x => x.Id == siete)).ImporteIva == 0, "Una alícuota explícita cero debe persistir sin reemplazarse por el default 21.");
    }
    finally
    {
        SqlConnection.ClearAllPools();
        await using var borrar = conexion.CreateCommand(); borrar.CommandText = $"ALTER DATABASE [{nombre}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{nombre}];"; await borrar.ExecuteNonQueryAsync();
        if (Directory.Exists(directorio) && Path.GetFileName(directorio) == nombre && Path.GetFullPath(directorio).StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase)) Directory.Delete(directorio, recursive: true);
    }
}
Console.WriteLine($"ARCA: {verificaciones} comprobaciones correctas. Servicios de ARCA simulados; no se emitieron facturas reales.");

sealed class Factory(DbContextOptions<AppDbContext> options) : IDbContextFactory<AppDbContext> { public AppDbContext CreateDbContext() => new(options); }
sealed class HttpFactory(HttpMessageHandler handler) : IHttpClientFactory { public HttpClient CreateClient(string name) => new(handler, disposeHandler: false); }
sealed class Simulador : HttpMessageHandler
{
    private readonly Dictionary<(string Ambiente, int Tipo, long Numero), XElement> emitidos = new();
    public int Envios, Wsaa; public bool CortarDespuesAutorizar, UltimoInvalido, RechazarEnvio;
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var entrada = XDocument.Parse(await request.Content!.ReadAsStringAsync(cancellationToken));
        string? V(string nombre) => ArcaWsfeClient.Valor(entrada, nombre);
        string ambiente = request.RequestUri!.Host.Contains("homo") ? "H" : "P";
        string respuesta;
        if (request.RequestUri.Host.Contains("wsaa"))
        {
            Wsaa++; var ticket = new XElement("loginTicketResponse", new XElement("header", new XElement("generationTime", DateTime.Now.ToString("O")), new XElement("expirationTime", DateTime.Now.AddHours(1).ToString("O"))), new XElement("credentials", new XElement("token", "token-prueba"), new XElement("sign", "sign-prueba")));
            respuesta = new XElement("loginCmsResponse", new XElement("loginCmsReturn", ticket.ToString())).ToString();
        }
        else if (entrada.Descendants().Any(x => x.Name.LocalName == "FEParamGetPtosVenta")) respuesta = "<ResultGet><PtoVenta><Nro>1</Nro><EmisionTipo>CAE</EmisionTipo><Bloqueado>N</Bloqueado><FchBaja>NULL</FchBaja></PtoVenta></ResultGet>";
        else if (entrada.Descendants().Any(x => x.Name.LocalName == "FECompUltimoAutorizado")) respuesta = UltimoInvalido ? "<Result/>" : $"<Result><CbteNro>{emitidos.Keys.Where(x => x.Ambiente == ambiente && x.Tipo == int.Parse(V("CbteTipo")!)).Select(x => x.Numero).DefaultIfEmpty().Max()}</CbteNro></Result>";
        else if (entrada.Descendants().Any(x => x.Name.LocalName == "FECompConsultar")) respuesta = emitidos.TryGetValue((ambiente, int.Parse(V("CbteTipo")!), long.Parse(V("CbteNro")!)), out var r) ? r.ToString() : "<Result><Errors><Err><Code>602</Code><Msg>No existen datos</Msg></Err></Errors></Result>";
        else
        {
            Envios++; var tipo = int.Parse(V("CbteTipo")!); var numero = long.Parse(V("CbteDesde")!);
            if (RechazarEnvio) { RechazarEnvio = false; return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("<Result><FECAEDetResponse><Resultado>R</Resultado><Observaciones><Obs><Code>10015</Code><Msg>Rechazo simulado</Msg></Obs></Observaciones></FECAEDetResponse></Result>") }; }
            var datos = entrada.Descendants().First(x => x.Name.LocalName == "FECAEDetRequest").Elements().Select(x => new XElement(x));
            var resultado = new XElement("ResultGet", datos, new XElement("PtoVta", V("PtoVta")), new XElement("CbteTipo", tipo), new XElement("Resultado", "A"), new XElement("CodAutorizacion", "12345678901234"), new XElement("EmisionTipo", "CAE"), new XElement("FchVto", DateTime.Today.AddDays(10).ToString("yyyyMMdd")));
            emitidos[(ambiente, tipo, numero)] = resultado;
            if (CortarDespuesAutorizar) { CortarDespuesAutorizar = false; throw new TaskCanceledException("Corte simulado después de autorizar."); }
            var encabezado = new XElement("FeCabResp", new XElement("Cuit", V("Cuit")), new XElement("PtoVta", V("PtoVta")), new XElement("CbteTipo", tipo), new XElement("Resultado", "A"));
            var detalle = new XElement("FECAEDetResponse", new XElement("Concepto", V("Concepto")), new XElement("DocTipo", V("DocTipo")), new XElement("DocNro", V("DocNro")), new XElement("CbteDesde", numero), new XElement("CbteHasta", numero), new XElement("CbteFch", V("CbteFch")), new XElement("Resultado", "A"), new XElement("CAE", "12345678901234"), new XElement("CAEFchVto", DateTime.Today.AddDays(10).ToString("yyyyMMdd")));
            respuesta = new XElement("Result", encabezado, detalle).ToString();
        }
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(respuesta) };
    }
}

sealed class JsPrueba : IJSRuntime
{
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => ValueTask.FromResult(default(TValue)!);
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken token, object?[]? args) => ValueTask.FromResult(default(TValue)!);
}
sealed class NavegacionPrueba : NavigationManager
{
    public NavegacionPrueba() => Initialize("http://localhost/", "http://localhost/");
    protected override void NavigateToCore(string uri, bool forceLoad) { }
}

// Adaptador solo para pruebas: renderiza el HTML de componentes InteractiveServer
// sin abrir un circuito ni ejecutar eventos. No cambia el modo de la aplicación.
sealed class RenderizadorPruebas(IServiceProvider servicios, ILoggerFactory logger)
    : Microsoft.AspNetCore.Components.HtmlRendering.Infrastructure.StaticHtmlRenderer(servicios, logger)
{
    protected override IComponent ResolveComponentForRenderMode(Type tipo, int? parentId, IComponentActivator activador, IComponentRenderMode modo) => activador.CreateInstance(tipo);
}
