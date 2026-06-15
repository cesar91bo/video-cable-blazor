using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoCable.Domain.Entities;

namespace VideoCable.Infrastructure.Configurations;

public class MovimientoCajaConfiguration : IEntityTypeConfiguration<MovimientoCaja>
{
    public void Configure(EntityTypeBuilder<MovimientoCaja> builder)
    {
        builder.ToTable("MovimientosCaja");

        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.CajaDiaria)
            .WithMany(x => x.Movimientos)
            .HasForeignKey(x => x.CajaDiariaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Pago)
            .WithMany()
            .HasForeignKey(x => x.PagoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Monto)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.Concepto)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(x => x.MotivoAnulacion)
            .HasMaxLength(500);

        builder.HasIndex(x => new { x.CajaDiariaId, x.Fecha });

        builder.HasIndex(x => x.PagoId);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();
    }
}