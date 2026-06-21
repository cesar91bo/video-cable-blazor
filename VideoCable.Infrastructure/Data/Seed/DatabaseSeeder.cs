using Microsoft.EntityFrameworkCore;
using VideoCable.Domain.Entities;
using VideoCable.Domain.Enums;

namespace VideoCable.Infrastructure.Data.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await context.Database.MigrateAsync();

        // Solo se asegura la empresa base.
        // No cargamos servicios ni cajas porque esos datos se van a configurar manualmente en el cliente.
        await SeedEmpresaAsync(context);

        await context.SaveChangesAsync();
    }

    private static async Task SeedEmpresaAsync(AppDbContext context)
{
    var empresa = await context.Empresas.FirstOrDefaultAsync(x => x.Cuit == "20202233954");

    if (empresa is null)
    {
        empresa = new Empresa
        {
            CreatedAt = DateTime.Now
        };

        context.Empresas.Add(empresa);
    }

    empresa.RazonSocial = "VIDEO CABLE PAMPA DEL INDIO";
    empresa.NombreFantasia = "Video Cable";
    empresa.Cuit = "20202233954";
    empresa.CondicionIva = CondicionIvaEmpresa.ResponsableInscripto;
    empresa.IngresosBrutos = "3531";
    empresa.InicioActividades = null;
    empresa.Direccion = "Luis Pessano 91";
    empresa.Localidad = "Pampa del Indio";
    empresa.Provincia = "Chaco";
    empresa.Telefono = "3725494110";
    empresa.Email = null;
    empresa.Activa = true;

    empresa.UpdatedAt = DateTime.Now;
}

    private static async Task SeedServiciosAsync(AppDbContext context)
{
    var servicios = new List<ServicioPlan>
    {
        new()
        {
            Codigo = "INT-INTERMEDIO",
            Descripcion = "Internet Intermedio",
            TipoServicio = TipoServicio.Internet,
            TipoCobro = TipoCobroServicio.Mensual,
            PrecioActual = 22000,
            Activo = true,
            CreatedAt = DateTime.Now
        },
        new()
        {
            Codigo = "INT-FULL",
            Descripcion = "Internet Full",
            TipoServicio = TipoServicio.Internet,
            TipoCobro = TipoCobroServicio.Mensual,
            PrecioActual = 27000,
            Activo = true,
            CreatedAt = DateTime.Now
        },
        new()
        {
            Codigo = "TV-CABLE",
            Descripcion = "Televisión por Cable",
            TipoServicio = TipoServicio.Video,
            TipoCobro = TipoCobroServicio.Mensual,
            PrecioActual = 20000,
            Activo = true,
            CreatedAt = DateTime.Now
        },
        new()
        {
            Codigo = "COMBO-BASICO",
            Descripcion = "Combo Básico Internet + TV",
            TipoServicio = TipoServicio.InternetTv,
            TipoCobro = TipoCobroServicio.Mensual,
            PrecioActual = 32000,
            Activo = true,
            CreatedAt = DateTime.Now
        },
        new()
        {
            Codigo = "COMBO-INTERMEDIO",
            Descripcion = "Internet Intermedio + TV",
            TipoServicio = TipoServicio.InternetTv,
            TipoCobro = TipoCobroServicio.Mensual,
            PrecioActual = 35000,
            Activo = true,
            CreatedAt = DateTime.Now
        },
        new()
        {
            Codigo = "COMBO-FULL",
            Descripcion = "Internet Full + TV",
            TipoServicio = TipoServicio.InternetTv,
            TipoCobro = TipoCobroServicio.Mensual,
            PrecioActual = 40000,
            Activo = true,
            CreatedAt = DateTime.Now
        },

        new()
        {
            Codigo = "CONEXION-TV",
            Descripcion = "Derecho de Conexión TV",
            TipoServicio = TipoServicio.Video,
            TipoCobro = TipoCobroServicio.Puntual,
            PrecioActual = 25000,
            Activo = true,
            CreatedAt = DateTime.Now
        },
        new()
        {
            Codigo = "CONEXION-INTERNET",
            Descripcion = "Derecho de Conexión Internet",
            TipoServicio = TipoServicio.Internet,
            TipoCobro = TipoCobroServicio.Puntual,
            PrecioActual = 30000,
            Activo = true,
            CreatedAt = DateTime.Now
        },
        new()
        {
            Codigo = "CONEXION-COMBO",
            Descripcion = "Derecho de Conexión Combo TV + Internet",
            TipoServicio = TipoServicio.InternetTv,
            TipoCobro = TipoCobroServicio.Puntual,
            PrecioActual = 45000,
            Activo = true,
            CreatedAt = DateTime.Now
        },
        new()
        {
            Codigo = "RECONEXION",
            Descripcion = "Reconexión",
            TipoServicio = TipoServicio.Otro,
            TipoCobro = TipoCobroServicio.Puntual,
            PrecioActual = 0,
            Activo = true,
            CreatedAt = DateTime.Now
        }
    };

    foreach (var servicioSeed in servicios)
    {
        var servicio = await context.ServiciosPlan
            .FirstOrDefaultAsync(x => x.Codigo == servicioSeed.Codigo);

        if (servicio is null)
        {
            context.ServiciosPlan.Add(servicioSeed);

            context.ServiciosPrecio.Add(new ServicioPrecio
            {
                ServicioPlan = servicioSeed,
                Precio = servicioSeed.PrecioActual,
                VigenteDesde = DateTime.Today,
                Motivo = "Carga inicial desde sistema anterior",
                CreatedAt = DateTime.Now
            });
        }
        else
        {
            servicio.Descripcion = servicioSeed.Descripcion;
            servicio.TipoServicio = servicioSeed.TipoServicio;
            servicio.TipoCobro = servicioSeed.TipoCobro;
            servicio.PrecioActual = servicioSeed.PrecioActual;
            servicio.Activo = servicioSeed.Activo;
            servicio.UpdatedAt = DateTime.Now;
        }
    }
}

    private static async Task SeedCajasAsync(AppDbContext context)
{
    // Borro las cajas genéricas iniciales si todavía existen y no tienen uso.
    var codigosGenericos = new[] { "CJA-001", "CJA-002", "CJA-003" };

    var cajasGenericas = await context.CajasDistribucion
        .Where(x => codigosGenericos.Contains(x.Codigo))
        .ToListAsync();

    context.CajasDistribucion.RemoveRange(cajasGenericas);

    var cajas = new[]
    {
        new { Codigo = "CAJA-001", Descripcion = "Caja 1", Direccion = "Av. 25 de Mayo y Acceso Norte" },
        new { Codigo = "CAJA-040", Descripcion = "Caja 40", Direccion = "Av. 25 de Mayo y Suipacha" },
        new { Codigo = "CAJA-032", Descripcion = "Caja 32", Direccion = "Chacabuco y Luis Pessano" },
        new { Codigo = "CAJA-006", Descripcion = "Caja 6", Direccion = "Balbuena Valdez y Suipacha" },
        new { Codigo = "CAJA-019", Descripcion = "Caja 19", Direccion = "San Martin y Catalina Alsina" },
        new { Codigo = "CAJA-042", Descripcion = "Caja 42", Direccion = "Av. Casique Mayordomo y Catalina Alsina" },
        new { Codigo = "CAJA-014", Descripcion = "Caja 14", Direccion = "San Martin y Raul Silvestri" },
        new { Codigo = "CAJA-022", Descripcion = "Caja 22", Direccion = "Catalina Alsina y Malvinas Argentinas" },
        new { Codigo = "CAJA-016", Descripcion = "Caja 16", Direccion = "Raul Exner" },
        new { Codigo = "CAJA-021", Descripcion = "Caja 21", Direccion = "Vuelta de Obligado y Catalina Alsina" },
        new { Codigo = "CAJA-030", Descripcion = "Caja 30", Direccion = "Maipu y Misiones" },

        // Caja 18 no se carga porque tenía FechaBaja en el sistema anterior.

        new { Codigo = "CAJA-059", Descripcion = "Caja 59", Direccion = "Maipu y Casique Pedro Martinez" },
        new { Codigo = "CAJA-060", Descripcion = "Caja 60", Direccion = "Suipacha y Corrientes" },
        new { Codigo = "CAJA-033", Descripcion = "Caja 33", Direccion = "Maipu y Luis Pessano" },
        new { Codigo = "CAJA-010", Descripcion = "Caja 10", Direccion = "Av. 25 de Mayo y Malvinas Argentinas" },
        new { Codigo = "CAJA-011", Descripcion = "Caja 11", Direccion = "Malvinas Argentinas y Raul Silvestri" },
        new { Codigo = "CAJA-061", Descripcion = "Caja 61", Direccion = "Belgrano" },
        new { Codigo = "CAJA-046", Descripcion = "Caja 46", Direccion = "Maipu y Raul Silvestri" },
        new { Codigo = "CAJA-024", Descripcion = "Caja 24", Direccion = "Malvinas Argentinas" },
        new { Codigo = "CAJA-054", Descripcion = "Caja 54", Direccion = "Vuelta de Obligado y Casique Pedro Martinez" },
        new { Codigo = "CAJA-028", Descripcion = "Caja 28", Direccion = "Belgrano y Luis Pessano" },
        new { Codigo = "CAJA-057", Descripcion = "Caja 57", Direccion = "San Martin y Casique Pedro Martinez" },
        new { Codigo = "CAJA-043", Descripcion = "Caja 43", Direccion = "Maipu y Catalina Alsina" },
        new { Codigo = "CAJA-023", Descripcion = "Caja 23", Direccion = "Av. 25 de Mayo y Belgrano" },
        new { Codigo = "CAJA-035", Descripcion = "Caja 35", Direccion = "Malvinas Argentinas y Luis Pessano" },
        new { Codigo = "CAJA-048", Descripcion = "Caja 48", Direccion = "Av. 25 de Mayo y Chacabuco" },
        new { Codigo = "CAJA-051", Descripcion = "Caja 51", Direccion = "Suipacha y Luis Pessano" },
        new { Codigo = "CAJA-056", Descripcion = "Caja 56", Direccion = "Belgrano y Casique Pedro Martinez" },
        new { Codigo = "CAJA-017", Descripcion = "Caja 17", Direccion = "Chacabuco y Raul Exner" },
        new { Codigo = "CAJA-020", Descripcion = "Caja 20", Direccion = "Belgrano y Catalina Alsina" },
        new { Codigo = "CAJA-045", Descripcion = "Caja 45", Direccion = "A media cuadra Av. 25 de mayo, Calle Maipu" },
        new { Codigo = "CAJA-026", Descripcion = "Caja 26", Direccion = "Av. Carlos Schulz y Luis Pessano" },
        new { Codigo = "CAJA-039", Descripcion = "Caja 39", Direccion = "A media cuadra de la Chacabuco por Raul Silvestri" },
        new { Codigo = "CAJA-013", Descripcion = "Caja 13", Direccion = "Belgrano y Raul Exner" },
        new { Codigo = "CAJA-005", Descripcion = "Caja 5", Direccion = "Acceso Norte" },
        new { Codigo = "CAJA-058", Descripcion = "Caja 58", Direccion = "Chacabuco y Casique Pedro Martinez" },
        new { Codigo = "CAJA-002", Descripcion = "Caja 2", Direccion = "Av. 25 de Mayo y Acceso Norte" },
        new { Codigo = "CAJA-049", Descripcion = "Caja 49", Direccion = "A media cuadra de Av. 25 de Mayo por Chacabuco" },
        new { Codigo = "CAJA-008", Descripcion = "Caja 8", Direccion = "Malvinas Argentinas" },
        new { Codigo = "CAJA-044", Descripcion = "Caja 44", Direccion = "Suipacha y Catalina Alsina" },
        new { Codigo = "CAJA-015", Descripcion = "Caja 15", Direccion = "San Martin y Raul Exner" },
        new { Codigo = "CAJA-062", Descripcion = "Caja 62", Direccion = "Av. 25 de Mayo y Av. Carlos Schulz" },
        new { Codigo = "CAJA-053", Descripcion = "Caja 53", Direccion = "Av. Carlos Schulz y Casique Pedro Martinez" },
        new { Codigo = "CAJA-029", Descripcion = "Caja 29", Direccion = "Chacabuco y Balbuena Valdez" },
        new { Codigo = "CAJA-004", Descripcion = "Caja 4", Direccion = "Acceso Norte" },
        new { Codigo = "CAJA-012", Descripcion = "Caja 12", Direccion = "A media cuadra Belgrano por Raul Silvestri" },
        new { Codigo = "CAJA-055", Descripcion = "Caja 55", Direccion = "Malvinas Argentinas y Casique Pedro Martinez" },
        new { Codigo = "CAJA-036", Descripcion = "Caja 36", Direccion = "Av. 25 de Mayo y Vuelta de Obligado" },
        new { Codigo = "CAJA-027", Descripcion = "Caja 27", Direccion = "Vuelta de Obligado y Luis Pessano" },
        new { Codigo = "CAJA-050", Descripcion = "Caja 50", Direccion = "Chacabuco y Catalina Alsina" },
        new { Codigo = "CAJA-041", Descripcion = "Caja 41", Direccion = "Av. 25 de Mayo y Maipu" },
        new { Codigo = "CAJA-034", Descripcion = "Caja 34", Direccion = "Av. 25 de Mayo y Av. Cacique Mayordomo" },
        new { Codigo = "CAJA-VIDEO", Descripcion = "Caja para abonados del video", Direccion = "Clientes de Video" },
        new { Codigo = "CAJA-003", Descripcion = "Caja 3", Direccion = "Acceso Norte, Av 25 de Mayo" }
    };

    foreach (var cajaSeed in cajas)
    {
        var caja = await context.CajasDistribucion
            .FirstOrDefaultAsync(x => x.Codigo == cajaSeed.Codigo);

        if (caja is null)
        {
            caja = new CajaDistribucion
            {
                Codigo = cajaSeed.Codigo,
                CreatedAt = DateTime.Now
            };

            context.CajasDistribucion.Add(caja);
        }

        caja.Descripcion = cajaSeed.Descripcion;
        caja.Direccion = cajaSeed.Direccion;
        caja.Zona = null;
        caja.Latitud = null;
        caja.Longitud = null;
        caja.Activa = true;
        caja.UpdatedAt = DateTime.Now;
    }
}
}