using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoCable.Domain.Entities;
using VideoCable.Domain.Enums;

namespace VideoCable.Infrastructure.Configurations;

public class SuscripcionConfiguration : IEntityTypeConfiguration<Suscripcion>
{
    public void Configure(EntityTypeBuilder<Suscripcion> builder)
    {
        builder.ToTable("Suscripciones");

        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Cliente)
            .WithMany()
            .HasForeignKey(x => x.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ServicioPlan)
            .WithMany()
            .HasForeignKey(x => x.ServicioPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CajaDistribucion)
            .WithMany()
            .HasForeignKey(x => x.CajaDistribucionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.PrecioActualSnapshot)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.DireccionInstalacion)
            .HasMaxLength(250);

        builder.Property(x => x.LocalidadInstalacion)
            .HasMaxLength(100);

        builder.Property(x => x.ReferenciaInstalacion)
            .HasMaxLength(500);

        builder.Property(x => x.Observaciones)
            .HasMaxLength(1000);

        builder.HasIndex(x => x.ClienteId);
        builder.HasIndex(x => x.EstadoActual);

        builder.HasIndex(x => new
        {
            x.ClienteId,
            x.ServicioPlanId,
            x.CajaDistribucionId
        })
            .IsUnique()
            .HasFilter($"[IsDeleted] = 0 AND [EstadoActual] = {(int)EstadoSuscripcionTipo.Activo}");

        builder.Property(x => x.RowVersion)
            .IsRowVersion();
    }
}