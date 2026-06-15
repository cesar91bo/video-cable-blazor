using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoCable.Domain.Entities;

namespace VideoCable.Infrastructure.Configurations;

public class FacturaConfiguration : IEntityTypeConfiguration<Factura>
{
    public void Configure(EntityTypeBuilder<Factura> builder)
    {
        builder.ToTable("Facturas");

        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Empresa)
            .WithMany()
            .HasForeignKey(x => x.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Cliente)
            .WithMany()
            .HasForeignKey(x => x.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Suscripcion)
            .WithMany()
            .HasForeignKey(x => x.SuscripcionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Subtotal)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.ImporteIva)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.Total)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.NumeroComprobanteFormateado)
            .HasMaxLength(30);

        builder.Property(x => x.Observaciones)
            .HasMaxLength(1000);

        builder.Property(x => x.ClienteNombreSnapshot)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.ClienteDocumentoSnapshot)
            .HasMaxLength(30);

        builder.Property(x => x.ClienteCuitSnapshot)
            .HasMaxLength(11);

        builder.Property(x => x.ClienteCondicionIvaSnapshot)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ClienteDireccionSnapshot)
            .HasMaxLength(250);

        builder.Property(x => x.ServicioDescripcionSnapshot)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.ServicioTipoSnapshot)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.CajaDescripcionSnapshot)
            .HasMaxLength(150);

        builder.Property(x => x.EmpresaRazonSocialSnapshot)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.EmpresaCuitSnapshot)
            .IsRequired()
            .HasMaxLength(11);

        builder.Property(x => x.EmpresaCondicionIvaSnapshot)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.EmpresaDireccionSnapshot)
            .IsRequired()
            .HasMaxLength(250);

        builder.HasIndex(x => new { x.SuscripcionId, x.PeriodoAnio, x.PeriodoMes })
            .IsUnique();

        builder.HasIndex(x => new { x.ClienteId, x.PeriodoAnio, x.PeriodoMes });

        builder.HasIndex(x => x.EstadoCobranza);

        builder.HasIndex(x => x.EstadoFiscal);

        builder.HasIndex(x => new 
            { 
                x.EmpresaId, 
                x.PuntoVentaId, 
                x.TipoComprobante, 
                x.NumeroComprobante 
            })
            .IsUnique()
            .HasFilter("[NumeroComprobante] IS NOT NULL");

        builder.Property(x => x.RowVersion)
            .IsRowVersion();
    }
}