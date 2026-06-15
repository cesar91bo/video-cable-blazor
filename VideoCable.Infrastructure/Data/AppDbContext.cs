using Microsoft.EntityFrameworkCore;
using VideoCable.Domain.Entities;

namespace VideoCable.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<PuntoVenta> PuntosVenta => Set<PuntoVenta>();
    public DbSet<CertificadoEmpresa> CertificadosEmpresa => Set<CertificadoEmpresa>();

    public DbSet<Cliente> Clientes => Set<Cliente>();

    public DbSet<ServicioPlan> ServiciosPlan => Set<ServicioPlan>();
    public DbSet<ServicioPrecio> ServiciosPrecio => Set<ServicioPrecio>();

    public DbSet<CajaDistribucion> CajasDistribucion => Set<CajaDistribucion>();

    public DbSet<Suscripcion> Suscripciones => Set<Suscripcion>();
    public DbSet<SuscripcionEstadoHistorial> SuscripcionEstadosHistorial => Set<SuscripcionEstadoHistorial>();

    public DbSet<Factura> Facturas => Set<Factura>();
    public DbSet<FacturaItem> FacturaItems => Set<FacturaItem>();
    public DbSet<FacturaElectronica> FacturasElectronicas => Set<FacturaElectronica>();

    public DbSet<AfipToken> AfipTokens => Set<AfipToken>();

    public DbSet<Pago> Pagos => Set<Pago>();
    public DbSet<PagoDetalle> PagoDetalles => Set<PagoDetalle>();
    public DbSet<PagoAplicacion> PagoAplicaciones => Set<PagoAplicacion>();

    public DbSet<CajaDiaria> CajasDiarias => Set<CajaDiaria>();
    public DbSet<MovimientoCaja> MovimientosCaja => Set<MovimientoCaja>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}