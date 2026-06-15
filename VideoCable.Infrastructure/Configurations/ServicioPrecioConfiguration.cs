using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoCable.Domain.Entities;

namespace VideoCable.Infrastructure.Configurations;

public class ServicioPrecioConfiguration : IEntityTypeConfiguration<ServicioPrecio>
{
    public void Configure(EntityTypeBuilder<ServicioPrecio> builder)
    {
        builder.ToTable("ServiciosPrecio");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Precio)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.Motivo)
            .HasMaxLength(250);

        builder.HasOne(x => x.ServicioPlan)
            .WithMany()
            .HasForeignKey(x => x.ServicioPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.ServicioPlanId, x.VigenteDesde })
            .IsUnique();

        builder.HasIndex(x => new { x.ServicioPlanId, x.VigenteHasta });

        builder.Property(x => x.RowVersion)
            .IsRowVersion();
    }
}