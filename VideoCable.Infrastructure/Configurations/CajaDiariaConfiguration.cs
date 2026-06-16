using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoCable.Domain.Entities;
using VideoCable.Domain.Enums;

namespace VideoCable.Infrastructure.Configurations;

public class CajaDiariaConfiguration : IEntityTypeConfiguration<CajaDiaria>
{
    public void Configure(EntityTypeBuilder<CajaDiaria> builder)
    {
        builder.ToTable("CajasDiarias");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.MontoInicial)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.MontoFinalInformado)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.MontoSistema)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.Diferencia)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.ObservacionesApertura)
            .HasMaxLength(1000);

        builder.Property(x => x.ObservacionesCierre)
            .HasMaxLength(1000);

        builder.HasIndex(x => x.Estado);

        builder.HasIndex(x => x.FechaApertura);

        builder.HasIndex(x => x.Estado)
            .IsUnique()
            .HasFilter($"[Estado] = {(int)EstadoCajaDiaria.Abierta}");

        builder.Property(x => x.RowVersion)
            .IsRowVersion();
    }
}