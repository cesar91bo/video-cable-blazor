using VideoCable.Web.Services;
using VideoCable.Infrastructure.Services.Arca;

var comprobaciones = 0;
void Verificar(bool condicion, string mensaje)
{
    if (!condicion) throw new Exception(mensaje);
    comprobaciones++;
}

Verificar(ResumenReporte.InicioSemana(new DateTime(2026, 10, 11)) == new DateTime(2026, 10, 5), "El domingo debe corresponder a la semana anterior.");
Verificar(ResumenReporte.InicioSemana(new DateTime(2026, 10, 12)) == new DateTime(2026, 10, 12), "El lunes inicia una semana.");
var pago = new MovimientoReporte(1, new DateTime(2026, 9, 30, 23, 59, 59), 1, "1 - Ana", "", 100m);
var partes = ResumenReporte.Desglosar(pago, [("Cable", 30m), ("Cable", 20m), ("Internet", 40m)], "Sin asignar");
Verificar(partes.Sum(x => x.Importe) == 100m && partes.Last().Importe == 10m, "El saldo sin asignar debe reconciliar el pago.");
var servicios = ResumenReporte.Agrupar(partes, "servicio");
Verificar(servicios.Single(x => x.Etiqueta == "Cable").Operaciones == 1, "Varias facturas del mismo servicio no deben duplicar la cantidad de pagos.");
Verificar(servicios.Single(x => x.Etiqueta == "Cable").Importe == 50m, "Debe sumar ambas aplicaciones a cable.");
var medios = ResumenReporte.Desglosar(pago, [("Efectivo", 60m), ("Transferencia", 40m)], "Sin medio");
Verificar(medios.Count == 2 && medios.Sum(x => x.Importe) == 100m, "Un pago mixto debe conservar sus importes reales y el total.");
Verificar(ResumenReporte.Desglosar(pago, [], "Sin medio").Single().Importe == 100m, "Un pago sin detalles debe conservarse.");
var otros = new[] { pago, pago with { Id = 2, Fecha = new DateTime(2026, 10, 1), Importe = 50m }, pago with { Id = 3, Fecha = new DateTime(2026, 10, 5), Importe = -20m } };
var semanas = ResumenReporte.Agrupar(otros, "semana");
Verificar(semanas.Count == 2 && semanas[0].Importe == 150m && semanas[1].Importe == -20m, "Agrupar semanas debe atravesar límites de mes y mantener créditos negativos.");
var meses = ResumenReporte.Agrupar(otros, "mes");
Verificar(meses.Count == 2 && meses[0].Importe == 100m && meses[1].Importe == 30m, "Los meses deben conservar los saldos netos.");
var clientes = ResumenReporte.Agrupar([pago, pago with { Id = 2, ClienteId = 2 }], "cliente");
Verificar(clientes.Count == 2, "Clientes con igual nombre deben seguir separados por identidad.");
Verificar(ResumenReporte.Agrupar([], "dia").Count == 0, "El reporte vacío no debe generar filas.");
Verificar(ResumenReporte.Agrupar(partes, "dia").Single().Operaciones == 1, "Un pago desglosado no debe contarse varias veces en el día.");
Verificar(LimiteFacturacionMensual.Evaluar(null, 100m, 50m) is null, "Sin límite no debe existir aviso.");
Verificar(LimiteFacturacionMensual.Evaluar(0m, 100m, 50m) is null, "Cero debe desactivar el límite.");
Verificar(!LimiteFacturacionMensual.Evaluar(1000m, 700m, 99.99m)!.DebeAvisar, "Debajo del 80 % no debe avisar.");
Verificar(LimiteFacturacionMensual.Evaluar(1000m, 700m, 100m)!.DebeAvisar, "Debe avisar al alcanzar el 80 % con la próxima factura.");
Verificar(LimiteFacturacionMensual.Evaluar(1000m, 950m, 50m)!.Mensaje.Contains("alcanzaría"), "Debe distinguir alcanzar el límite.");
Verificar(LimiteFacturacionMensual.Evaluar(1000m, 950m, 50.01m)!.Mensaje.Contains("superaría"), "Debe distinguir superar el límite.");
Verificar(LimiteFacturacionMensual.Evaluar(1000m, 1100m, 10m)!.Disponible == 0m, "El disponible no debe ser negativo.");
Console.WriteLine($"Reportes y límite ARCA: {comprobaciones} comprobaciones correctas.");
