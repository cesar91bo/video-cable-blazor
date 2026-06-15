using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoCable.Domain.Entities;

namespace VideoCable.Infrastructure.Configurations;

public class ServicioPlanConfiguration : IEntityTypeConfiguration<ServicioPlan>
{
    public void Configure(EntityTypeBuilder<ServicioPlan> builder)
    {
        builder.ToTable("ServiciosPlan");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Codigo)
            .HasMaxLength(50);

        builder.Property(x => x.Descripcion)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasIndex(x => x.Descripcion)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(x => x.TipoServicio);

        builder.Property(x => x.PrecioActual)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasIndex(x => x.TipoCobro);
    }
}