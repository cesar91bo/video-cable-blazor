using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoCable.Domain.Entities;

namespace VideoCable.Infrastructure.Configurations;

public class PuntoVentaConfiguration : IEntityTypeConfiguration<PuntoVenta>
{
    public void Configure(EntityTypeBuilder<PuntoVenta> builder)
    {
        builder.ToTable("PuntosVenta");

        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Empresa)
            .WithMany()
            .HasForeignKey(x => x.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Descripcion)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => new { x.EmpresaId, x.Ambiente, x.Numero })
            .IsUnique();

        builder.Property(x => x.RowVersion)
            .IsRowVersion();
    }
}