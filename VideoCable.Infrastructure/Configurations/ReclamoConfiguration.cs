using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoCable.Domain.Entities;

namespace VideoCable.Infrastructure.Configurations;

public class ReclamoConfiguration : IEntityTypeConfiguration<Reclamo>
{
    public void Configure(EntityTypeBuilder<Reclamo> builder)
    {
        builder.ToTable("Reclamos");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Descripcion)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.ObservacionesInternas)
            .HasMaxLength(1000);

        builder.Property(x => x.Resolucion)
            .HasMaxLength(1000);

        builder.Property(x => x.Tipo)
            .HasConversion<int>();

        builder.Property(x => x.Prioridad)
            .HasConversion<int>();

        builder.Property(x => x.Estado)
            .HasConversion<int>();

        builder.HasOne(x => x.Cliente)
            .WithMany()
            .HasForeignKey(x => x.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Suscripcion)
            .WithMany()
            .HasForeignKey(x => x.SuscripcionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CajaDistribucion)
            .WithMany()
            .HasForeignKey(x => x.CajaDistribucionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ClienteId);

        builder.HasIndex(x => x.Estado);

        builder.HasIndex(x => x.FechaReclamo);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();
    }
}