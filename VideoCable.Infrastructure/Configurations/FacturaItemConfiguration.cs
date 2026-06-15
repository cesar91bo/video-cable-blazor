using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoCable.Domain.Entities;

namespace VideoCable.Infrastructure.Configurations;

public class FacturaItemConfiguration : IEntityTypeConfiguration<FacturaItem>
{
    public void Configure(EntityTypeBuilder<FacturaItem> builder)
    {
        builder.ToTable("FacturaItems");

        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Factura)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.FacturaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Suscripcion)
            .WithMany()
            .HasForeignKey(x => x.SuscripcionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ServicioPlan)
            .WithMany()
            .HasForeignKey(x => x.ServicioPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Descripcion)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(x => x.Cantidad)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.PrecioUnitario)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.AlicuotaIva)
            .HasColumnType("decimal(5,2)");

        builder.Property(x => x.Subtotal)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.ImporteIva)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.Total)
            .HasColumnType("decimal(18,2)");

        builder.HasIndex(x => x.FacturaId);

        builder.HasIndex(x => x.SuscripcionId);
    }
}