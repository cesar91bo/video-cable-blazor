using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoCable.Domain.Entities;

namespace VideoCable.Infrastructure.Configurations;

public class FacturaElectronicaConfiguration : IEntityTypeConfiguration<FacturaElectronica>
{
    public void Configure(EntityTypeBuilder<FacturaElectronica> builder)
    {
        builder.ToTable("FacturasElectronicas");

        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Factura)
            .WithOne()
            .HasForeignKey<FacturaElectronica>(x => x.FacturaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.CAE)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Resultado)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Observaciones)
            .HasMaxLength(2000);

        builder.Property(x => x.Errores)
            .HasMaxLength(2000);

        builder.Property(x => x.RequestResumen)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.ResponseResumen)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.QrUrl)
            .HasMaxLength(1000);

        builder.HasIndex(x => x.FacturaId)
            .IsUnique();

        builder.HasIndex(x => x.CAE);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();
    }
}