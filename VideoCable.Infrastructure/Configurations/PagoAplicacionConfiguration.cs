using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoCable.Domain.Entities;

namespace VideoCable.Infrastructure.Configurations;

public class PagoAplicacionConfiguration : IEntityTypeConfiguration<PagoAplicacion>
{
    public void Configure(EntityTypeBuilder<PagoAplicacion> builder)
    {
        builder.ToTable("PagoAplicaciones");

        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Pago)
            .WithMany(x => x.Aplicaciones)
            .HasForeignKey(x => x.PagoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Factura)
            .WithMany(x => x.PagoAplicaciones)
            .HasForeignKey(x => x.FacturaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.MontoAplicado)
            .HasColumnType("decimal(18,2)");

        builder.HasIndex(x => new { x.PagoId, x.FacturaId })
            .IsUnique();

        builder.HasIndex(x => x.FacturaId);
    }
}