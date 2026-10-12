using System.Globalization;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using VideoCable.Domain.Entities;
using VideoCable.Domain.Enums;
using VideoCable.Infrastructure.Data;
namespace VideoCable.Infrastructure.Services.Arca;

public sealed class ArcaFacturaService(IDbContextFactory<AppDbContext> factory, ArcaAuthService authService, ArcaWsfeClient wsfe)
{
    public async Task<string> PrepararFacturaAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        var f = await Cargar(db, id);
        await using var bloqueo = await ArcaSqlLock.Adquirir(db, $"ARCA-EMISION:{f.Empresa.Cuit}:{f.Empresa.AmbienteArca}");
        await db.Entry(f).ReloadAsync();
        if (f.IsDeleted || f.EstadoCobranza == EstadoCobranzaFactura.Anulada || f.EstadoFiscal == EstadoFiscalFactura.Autorizada)
            throw new InvalidOperationException("No puede modificar una factura anulada, eliminada o autorizada.");
        if (await db.FacturasElectronicas.AnyAsync(x => x.FacturaId == id && x.NumeroSolicitado != null && x.Resultado != "R"))
            throw new InvalidOperationException("La factura tiene un intento guardado. Recupere su estado antes de modificarla.");
        ArcaFiscalRules.Preparar(f, DateTime.Today);
        await db.SaveChangesAsync();
        return $"Preparada como {f.TipoComprobante} para {f.AmbienteArca}. Todavía no fue enviada a ARCA.";
    }

    public async Task<string> AutorizarFacturaAsync(int facturaId, AmbienteFiscal ambiente, bool continuarConAvisoLimite = false)
    {
        if (!Enum.IsDefined(ambiente)) throw new InvalidOperationException("Ambiente ARCA no válido.");
        await using var db = await factory.CreateDbContextAsync();
        var f = await Cargar(db, facturaId);
        await using var bloqueo = await ArcaSqlLock.Adquirir(db, $"ARCA-EMISION:{f.Empresa.Cuit}:{ambiente}");
        await db.Entry(f).ReloadAsync();
        if (f.TipoComprobante == TipoComprobante.FacturaX) return "La factura X es interna y no se envía a ARCA.";
        if (f.AmbienteArca.HasValue && f.AmbienteArca != ambiente) throw new InvalidOperationException($"Esta factura pertenece a {f.AmbienteArca}. No se puede reutilizar en {ambiente}.");
        if (f.EstadoFiscal == EstadoFiscalFactura.Autorizada) return $"Ya autorizada en {f.AmbienteArca}. No se emitió nuevamente.";
        if (f.IsDeleted || f.EstadoFiscal == EstadoFiscalFactura.Anulada || f.EstadoCobranza == EstadoCobranzaFactura.Anulada) throw new InvalidOperationException("Factura anulada o eliminada.");
        if (f.Empresa.AmbienteArca != ambiente) throw new InvalidOperationException("El ambiente no coincide con Configuración ARCA.");
        var intento = await db.FacturasElectronicas.SingleOrDefaultAsync(x => x.FacturaId == f.Id);
        var recuperar = intento?.NumeroSolicitado is not null && intento.Resultado != "R";
        if (!recuperar && await db.FacturasElectronicas.AnyAsync(x => x.Factura.EmpresaId == f.EmpresaId && x.Ambiente == ambiente && x.FacturaId != f.Id && (x.Resultado == "P" || x.Resultado == "U")))
            throw new InvalidOperationException("Hay una emisión pendiente de recuperación. Reintente Autorizar ARCA en esa factura antes de emitir otra.");
        PuntoVenta pv; long numero; CredencialesArca auth;
        if (recuperar)
        {
            if (intento!.Ambiente != ambiente) throw new InvalidOperationException("El intento pertenece a otro ambiente.");
            pv = await db.PuntosVenta.SingleAsync(x => x.Id == intento.PuntoVentaSolicitadoId);
            if (pv.EmpresaId != f.EmpresaId || pv.Ambiente != ambiente) throw new InvalidOperationException("Se modificó el punto de venta del intento. Restaure su configuración.");
            numero = intento.NumeroSolicitado!.Value;
            if (intento.TipoComprobanteSolicitado != (int)f.TipoComprobante || intento.RequestResumen != ArcaWsfeClient.Solicitud(f, pv.Numero, numero).ToString(SaveOptions.DisableFormatting))
                throw new InvalidOperationException("Los datos cambiaron después del intento. No se reenviará un contenido diferente con el mismo número.");
            auth = await authService.ObtenerCredencialesAsync(f.EmpresaId, ambiente);
            var consulta = await wsfe.Consultar(auth, ambiente, pv.Numero, (int)f.TipoComprobante, numero);
            if (consulta is not null)
            {
                ValidarCoincidencia(f, pv.Numero, numero, consulta);
                await Aceptar(db, f, intento, pv, numero, ArcaWsfeClient.Valor(consulta, "CodAutorizacion")!, ArcaWsfeClient.Valor(consulta, "FchVto")!, consulta.ToString());
                return Resumen(f, intento, true);
            }
            if (intento.Resultado == "A" || await wsfe.Ultimo(auth, ambiente, pv.Numero, (int)f.TipoComprobante) != numero - 1)
                throw new InvalidOperationException("No se puede confirmar que el número reservado esté libre. No se emitió otra factura. Revise ARCA.");
        }
        else
        {
            if (f.TipoComprobante is not (TipoComprobante.FacturaA or TipoComprobante.FacturaB or TipoComprobante.FacturaC))
                throw new InvalidOperationException("Este flujo admite facturas A, B y C. Las notas requieren comprobantes asociados.");
            if (f.DocTipoReceptorArca is null) ArcaFiscalRules.Preparar(f, f.FechaEmision);
            pv = await db.PuntosVenta.SingleOrDefaultAsync(x => x.EmpresaId == f.EmpresaId && x.Ambiente == ambiente && x.Activo && x.Predeterminado && !x.IsDeleted && x.Tipo == TipoPuntoVenta.Electronico)
                ?? throw new InvalidOperationException($"Configure un punto de venta electrónico predeterminado de {ambiente}.");
            var aviso = await Aviso(db, f, ambiente);
            if (aviso is not null && !continuarConAvisoLimite) throw new AvisoLimiteFacturacionException(aviso);
            auth = await authService.ObtenerCredencialesAsync(f.EmpresaId, ambiente);
            await wsfe.ProbarPuntoVenta(auth, ambiente, pv.Numero);
            numero = await wsfe.Ultimo(auth, ambiente, pv.Numero, (int)f.TipoComprobante) + 1;
            if (numero > 99999999) throw new InvalidOperationException("Se agotó la numeración.");
            if (intento is null) { intento = new FacturaElectronica { FacturaId = f.Id, CreatedAt = DateTime.Now }; db.FacturasElectronicas.Add(intento); }
            intento.Ambiente = ambiente; intento.PuntoVentaSolicitadoId = pv.Id; intento.TipoComprobanteSolicitado = (int)f.TipoComprobante; intento.NumeroSolicitado = numero;
            intento.RequestResumen = ArcaWsfeClient.Solicitud(f, pv.Numero, numero).ToString(SaveOptions.DisableFormatting);
        }
        ArcaFiscalRules.ValidarFecha(f, DateTime.Today);
        if (f.Total <= 0 || f.Subtotal + f.ImporteIva != f.Total || f.Items.Sum(x => x.Subtotal) != f.Subtotal || f.Items.Sum(x => x.ImporteIva) != f.ImporteIva)
            throw new InvalidOperationException("Los importes fiscales no coinciden con los ítems.");
        intento!.Resultado = "P"; intento.Errores = null; intento.UpdatedAt = DateTime.Now;
        // La reserva y el pedido sin credenciales se guardan ANTES del envío.
        await db.SaveChangesAsync();
        try
        {
            var respuesta = await wsfe.Emitir(auth, ambiente, f, pv.Numero, numero);
            var detalle = respuesta.Descendants().FirstOrDefault(x => x.Name.LocalName == "FECAEDetResponse");
            var cae = detalle is null ? null : ArcaWsfeClient.Valor(detalle, "CAE");
            if (detalle is not null && ArcaWsfeClient.Valor(detalle, "Resultado") == "A" && !string.IsNullOrWhiteSpace(cae))
            {
                var cabecera = respuesta.Descendants().FirstOrDefault(x => x.Name.LocalName == "FeCabResp");
                if (cabecera is null || ArcaWsfeClient.Valor(cabecera, "Cuit") != f.EmpresaCuitSnapshot ||
                    ArcaWsfeClient.Valor(cabecera, "PtoVta") != pv.Numero.ToString() || ArcaWsfeClient.Valor(cabecera, "CbteTipo") != ((int)f.TipoComprobante).ToString() ||
                    ArcaWsfeClient.Valor(detalle, "CbteDesde") != numero.ToString() || ArcaWsfeClient.Valor(detalle, "CbteHasta") != numero.ToString() ||
                    ArcaWsfeClient.Valor(detalle, "DocTipo") != f.DocTipoReceptorArca.ToString() || ArcaWsfeClient.Valor(detalle, "DocNro") != f.DocNroReceptorArca.ToString() ||
                    ArcaWsfeClient.Valor(detalle, "CbteFch") != f.FechaEmision.ToString("yyyyMMdd"))
                    throw new InvalidOperationException("La respuesta autorizada no identifica el comprobante solicitado. Se debe recuperar mediante consulta.");
                await Aceptar(db, f, intento, pv, numero, cae, ArcaWsfeClient.Valor(detalle, "CAEFchVto")!, respuesta.ToString());
                return Resumen(f, intento, false);
            }
            if (!(detalle is not null && ArcaWsfeClient.Valor(detalle, "Resultado") == "R" || ArcaWsfeClient.Valor(respuesta, "Resultado") == "R"))
                throw new InvalidOperationException("Respuesta sin autorización ni rechazo concluyente.");
            intento.Resultado = "R"; intento.ResponseResumen = respuesta.ToString(); intento.Errores = ArcaWsfeClient.Mensajes(respuesta);
            f.EstadoFiscal = EstadoFiscalFactura.Rechazada;
            await db.SaveChangesAsync();
            return "ARCA rechazó la factura. No obtuvo CAE. " + intento.Errores;
        }
        catch (Exception ex)
        {
            if (intento.Resultado != "A") intento.Resultado = "U";
            intento.Errores = "Resultado pendiente de recuperación.";
            try { await db.SaveChangesAsync(); } catch {/* Sigue existiendo la reserva P guardada antes del envío. */}
            throw new InvalidOperationException("No se pudo completar o guardar la respuesta. No cree otra factura: vuelva a Autorizar ARCA en esta misma para recuperar el resultado.", ex);
        }
    }

    public static void ValidarCoincidencia(Factura f, int pv, long numero, XElement r)
    {
        bool Igual(string campo, string esperado) => ArcaWsfeClient.Valor(r, campo) == esperado;
        bool Importe(string campo, decimal esperado) => decimal.TryParse(ArcaWsfeClient.Valor(r, campo), NumberStyles.Number, CultureInfo.InvariantCulture, out var valor) && valor == esperado;
        if (!Igual("CbteTipo", ((int)f.TipoComprobante).ToString()) || !Igual("PtoVta", pv.ToString()) || !Igual("CbteDesde", numero.ToString()) || !Igual("CbteHasta", numero.ToString()) ||
            !Igual("Concepto", "2") || !Igual("CbteFch", f.FechaEmision.ToString("yyyyMMdd")) || !Igual("DocTipo", f.DocTipoReceptorArca.ToString()!) || !Igual("DocNro", f.DocNroReceptorArca.ToString()!) ||
            !Importe("ImpTotal", f.Total) || !Importe("ImpNeto", f.Subtotal) || !Importe("ImpIVA", f.ImporteIva) || !Igual("MonId", "PES") || !Importe("MonCotiz", 1m) ||
            !Importe("ImpTrib", 0m) || !Importe("ImpTotConc", 0m) || !Importe("ImpOpEx", 0m) || !Igual("Resultado", "A") ||
            !Igual("FchServDesde", new DateTime(f.PeriodoAnio, f.PeriodoMes, 1).ToString("yyyyMMdd")) ||
            !Igual("FchServHasta", new DateTime(f.PeriodoAnio, f.PeriodoMes, 1).AddMonths(1).AddDays(-1).ToString("yyyyMMdd")) ||
            !Igual("FchVtoPago", f.FechaVencimiento.ToString("yyyyMMdd")) ||
            string.IsNullOrWhiteSpace(ArcaWsfeClient.Valor(r, "CodAutorizacion")) || !Igual("EmisionTipo", "CAE"))
            throw new InvalidOperationException("El comprobante de ARCA no coincide con el intento local. No se vinculó ni emitió otro comprobante.");
    }

    private static async Task Aceptar(AppDbContext db, Factura f, FacturaElectronica i, PuntoVenta pv, long numero, string cae, string vencimiento, string respuesta)
    {
        if (cae.Length != 14 || !long.TryParse(cae, out var codigo) || codigo <= 0 || !DateTime.TryParseExact(vencimiento, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha))
            throw new InvalidOperationException("CAE o vencimiento no válido. Se debe recuperar el comprobante.");
        f.PuntoVentaId = pv.Id; f.PuntoVentaNumeroSnapshot = pv.Numero; f.NumeroComprobante = numero;
        f.NumeroComprobanteFormateado = $"{pv.Numero:00000}-{numero:00000000}"; f.EstadoFiscal = EstadoFiscalFactura.Autorizada; f.UpdatedAt = DateTime.Now;
        i.Resultado = "A"; i.CAE = cae; i.FechaVencimientoCAE = fecha; i.FechaAutorizacion = DateTime.Now;
        i.ResponseResumen = respuesta; i.Observaciones = ArcaWsfeClient.Mensajes(XDocument.Parse(respuesta)); i.Errores = null; i.QrUrl = ArcaQr.CrearUrl(f, cae); i.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();
    }

    private static string Resumen(Factura f, FacturaElectronica i, bool recuperada) =>
        $"Factura {(recuperada ? "recuperada sin emitir nuevamente" : "autorizada")} en {f.AmbienteArca}." + Environment.NewLine +
        $"Comprobante: {f.NumeroComprobanteFormateado}. Total: {f.Total:C2}. CAE: {i.CAE}. Vence: {i.FechaVencimientoCAE:dd/MM/yyyy}.";
    private static Task<Factura> Cargar(AppDbContext db, int id) => db.Facturas.Include(x => x.Empresa).Include(x => x.Cliente).Include(x => x.Items).SingleAsync(x => x.Id == id);
    private static async Task<string?> Aviso(AppDbContext db, Factura f, AmbienteFiscal ambiente)
    {
        var limite = await db.Empresas.AsNoTracking().Where(x => x.Id == f.EmpresaId).Select(x => x.LimiteFacturacionMensualArca).SingleAsync();
        if (limite is null or <= 0) return null;
        var inicio = new DateTime(f.FechaEmision.Year, f.FechaEmision.Month, 1); var fin = inicio.AddMonths(1);
        var acumulado = await db.FacturasElectronicas.AsNoTracking().Where(x => x.Resultado == "A" && x.CAE != "" && x.Factura.EmpresaId == f.EmpresaId && x.FacturaId != f.Id &&
            x.Factura.FechaEmision >= inicio && x.Factura.FechaEmision < fin && (x.Factura.TipoComprobante == TipoComprobante.FacturaA || x.Factura.TipoComprobante == TipoComprobante.FacturaB || x.Factura.TipoComprobante == TipoComprobante.FacturaC) &&
            (x.Ambiente == ambiente || x.Ambiente == null && db.PuntosVenta.Any(p => p.Id == x.Factura.PuntoVentaId && p.Ambiente == ambiente))).SumAsync(x => (decimal?)x.Factura.Total) ?? 0m;
        var estado = LimiteFacturacionMensual.Evaluar(limite, acumulado, f.Total)!;
        return estado.DebeAvisar ? $"Facturación ARCA de {inicio:MM/yyyy} · {ambiente}." + Environment.NewLine + estado.Mensaje : null;
    }
    public async Task<string> VerificarAutorizadaAsync(int facturaId)
    {
        await using var db = await factory.CreateDbContextAsync();
        var f = await Cargar(db, facturaId);
        var i = await db.FacturasElectronicas.SingleAsync(x => x.FacturaId == facturaId);
        if (f.EstadoFiscal != EstadoFiscalFactura.Autorizada || f.NumeroComprobante is null || f.PuntoVentaId is null || f.AmbienteArca is null)
            throw new InvalidOperationException("La factura no tiene una autorización y ambiente identificados.");
        var pv = await db.PuntosVenta.SingleAsync(x => x.Id == f.PuntoVentaId);
        if (pv.Ambiente != f.AmbienteArca || pv.EmpresaId != f.EmpresaId)
            throw new InvalidOperationException("El punto de venta no coincide con el ambiente o empresa del comprobante.");
        await using var bloqueo = await ArcaSqlLock.Adquirir(db, $"ARCA-EMISION:{f.Empresa.Cuit}:{f.AmbienteArca}");
        var auth = await authService.ObtenerCredencialesAsync(f.EmpresaId, f.AmbienteArca.Value);
        var r = await wsfe.Consultar(auth, f.AmbienteArca.Value, pv.Numero, (int)f.TipoComprobante, f.NumeroComprobante.Value)
            ?? throw new InvalidOperationException("ARCA no encontró ese comprobante. No se modificaron sus datos locales.");
        if (ArcaWsfeClient.Valor(r, "CodAutorizacion") != i.CAE || ArcaWsfeClient.Valor(r, "Resultado") != "A")
            throw new InvalidOperationException("El CAE consultado no coincide con el guardado.");
        if (f.DocTipoReceptorArca is null)
        {
            // Los comprobantes del flujo anterior podían conservar el total como
            // subtotal aunque se hubiera enviado IVA. Recuperar sus datos oficiales.
            var neto = decimal.Parse(ArcaWsfeClient.Valor(r, "ImpNeto")!, CultureInfo.InvariantCulture);
            var iva = decimal.Parse(ArcaWsfeClient.Valor(r, "ImpIVA")!, CultureInfo.InvariantCulture);
            if (neto + iva != f.Total || f.Items.Sum(x => x.Total) != f.Total || f.Items.Count == 0)
                throw new InvalidOperationException("Los importes consultados no coinciden con el total local.");
            var tasas = r.Descendants().Where(x => x.Name.LocalName == "AlicIva").Select(x => ArcaWsfeClient.Valor(x, "Id")).Distinct().ToList();
            if (tasas.Count > 1) throw new InvalidOperationException("La factura anterior tiene varias alícuotas. Requiere conciliar sus ítems antes de actualizar.");
            var tasa = tasas.FirstOrDefault() switch { "3" => 0m, "4" => 10.5m, "5" => 21m, "6" => 27m, "8" => 5m, "9" => 2.5m, null when iva == 0 => 0m, _ => throw new InvalidOperationException("Alícuota consultada no reconocida.") };
            f.DocTipoReceptorArca = int.Parse(ArcaWsfeClient.Valor(r, "DocTipo")!);
            f.DocNroReceptorArca = long.Parse(ArcaWsfeClient.Valor(r, "DocNro")!);
            f.CondicionIvaReceptorArca = int.TryParse(ArcaWsfeClient.Valor(r, "CondicionIVAReceptorId"), out var condicion) ? condicion : 5;
            f.Subtotal = neto; f.ImporteIva = iva;
            foreach (var item in f.Items)
            {
                item.AlicuotaIva = tasa; item.Subtotal = decimal.Round(item.Total / (1 + tasa / 100m), 2, MidpointRounding.AwayFromZero);
                item.ImporteIva = item.Total - item.Subtotal;
            }
            var ultimo = f.Items.Last(); var diferencia = neto - f.Items.Sum(x => x.Subtotal);
            ultimo.Subtotal += diferencia; ultimo.ImporteIva -= diferencia;
            foreach (var item in f.Items) item.PrecioUnitario = decimal.Round(item.Subtotal / item.Cantidad, 2);
            f.ClienteCuitSnapshot = f.DocTipoReceptorArca == 80 ? f.DocNroReceptorArca.ToString() : null;
            f.ClienteDocumentoSnapshot = f.DocTipoReceptorArca is not (80 or 99) ? f.DocNroReceptorArca.ToString() : null;
        }
        ValidarCoincidencia(f, pv.Numero, f.NumeroComprobante.Value, r);
        i.QrUrl = ArcaQr.CrearUrl(f, i.CAE); i.Ambiente = f.AmbienteArca; i.ResponseResumen = r.ToString();
        await db.SaveChangesAsync();
        return $"Comprobante verificado en {f.AmbienteArca}. Datos fiscales y QR conciliados; no se emitió otra factura.";
    }

    public async Task<string> ProbarConfiguracionAsync(int empresaId, AmbienteFiscal ambiente)
    {
        await using var db = await factory.CreateDbContextAsync();
        var pv = await db.PuntosVenta.AsNoTracking().SingleOrDefaultAsync(x => x.EmpresaId == empresaId && x.Ambiente == ambiente && !x.IsDeleted && x.Activo && x.Predeterminado && x.Tipo == TipoPuntoVenta.Electronico)
            ?? throw new InvalidOperationException("Configure un punto de venta electrónico predeterminado.");
        var auth = await authService.ObtenerCredencialesAsync(empresaId, ambiente);
        await wsfe.ProbarPuntoVenta(auth, ambiente, pv.Numero);
        return $"Conexión correcta en {ambiente}: certificado, WSAA y punto de venta {pv.Numero:00000} disponibles. Esta prueba no emite comprobantes.";
    }
    public async Task<string> ConsultarUltimoComprobanteAsync(int empresaId, AmbienteFiscal ambiente, int puntoVenta, int tipoComprobante) =>
        $"Último comprobante en {ambiente}: {await wsfe.Ultimo(await authService.ObtenerCredencialesAsync(empresaId, ambiente), ambiente, puntoVenta, tipoComprobante)}.";
    public Task<string> SolicitarCaeFacturaBPruebaAsync(int empresaId, AmbienteFiscal ambiente, int puntoVenta) =>
        Task.FromResult("Pruebe el CAE desde Facturación mensual en Homologación. No se emiten comprobantes sin registro local.");
}
