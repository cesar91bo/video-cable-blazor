using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoCable.Domain.Entities;

namespace VideoCable.Infrastructure.Configurations;

public class CajaDistribucionConfiguration : IEntityTypeConfiguration<CajaDistribucion>
{
    public void Configure(EntityTypeBuilder<CajaDistribucion> builder)
    {
        builder.ToTable("CajasDistribucion");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Codigo)
            .HasMaxLength(50);

        builder.Property(x => x.Descripcion)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Direccion)
            .HasMaxLength(250);

        builder.Property(x => x.Zona)
            .HasMaxLength(100);

        builder.Property(x => x.Latitud)
            .HasColumnType("decimal(9,6)");

        builder.Property(x => x.Longitud)
            .HasColumnType("decimal(9,6)");

        builder.HasIndex(x => x.Descripcion)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(x => x.Codigo)
            .IsUnique()
            .HasFilter("[Codigo] IS NOT NULL");

        builder.Property(x => x.RowVersion)
            .IsRowVersion();
    }
}