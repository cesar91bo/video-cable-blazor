using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoCable.Domain.Entities;

namespace VideoCable.Infrastructure.Configurations;

public class PagoConfiguration : IEntityTypeConfiguration<Pago>
{
    public void Configure(EntityTypeBuilder<Pago> builder)
    {
        builder.ToTable("Pagos");

        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Cliente)
            .WithMany()
            .HasForeignKey(x => x.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CajaDiaria)
            .WithMany(x => x.Pagos)
            .HasForeignKey(x => x.CajaDiariaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Total)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.Observaciones)
            .HasMaxLength(1000);

        builder.HasIndex(x => new { x.ClienteId, x.Fecha });

        builder.HasIndex(x => x.CajaDiariaId);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();
    }
}