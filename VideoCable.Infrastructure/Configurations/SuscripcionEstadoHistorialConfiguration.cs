using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoCable.Domain.Entities;

namespace VideoCable.Infrastructure.Configurations;

public class SuscripcionEstadoHistorialConfiguration : IEntityTypeConfiguration<SuscripcionEstadoHistorial>
{
    public void Configure(EntityTypeBuilder<SuscripcionEstadoHistorial> builder)
    {
        builder.ToTable("SuscripcionEstadosHistorial");

        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Suscripcion)
            .WithMany()
            .HasForeignKey(x => x.SuscripcionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Observaciones)
            .HasMaxLength(1000);

        builder.HasIndex(x => new { x.SuscripcionId, x.Fecha });
    }
}