using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoCable.Domain.Entities;

namespace VideoCable.Infrastructure.Configurations;

public class PagoDetalleConfiguration : IEntityTypeConfiguration<PagoDetalle>
{
    public void Configure(EntityTypeBuilder<PagoDetalle> builder)
    {
        builder.ToTable("PagoDetalles");

        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Pago)
            .WithMany(x => x.Detalles)
            .HasForeignKey(x => x.PagoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.Monto)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.Referencia)
            .HasMaxLength(150);

        builder.HasIndex(x => x.PagoId);
    }
}