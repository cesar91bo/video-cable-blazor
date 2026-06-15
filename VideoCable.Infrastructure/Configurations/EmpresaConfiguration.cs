using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoCable.Domain.Entities;

namespace VideoCable.Infrastructure.Configurations;

public class EmpresaConfiguration : IEntityTypeConfiguration<Empresa>
{
    public void Configure(EntityTypeBuilder<Empresa> builder)
    {
        builder.ToTable("Empresas");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RazonSocial)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.NombreFantasia)
            .HasMaxLength(200);

        builder.Property(x => x.Cuit)
            .IsRequired()
            .HasMaxLength(11);

        builder.HasIndex(x => x.Cuit)
            .IsUnique();

        builder.Property(x => x.IngresosBrutos)
            .HasMaxLength(50);

        builder.Property(x => x.Direccion)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(x => x.Localidad)
            .HasMaxLength(100);

        builder.Property(x => x.Provincia)
            .HasMaxLength(100);

        builder.Property(x => x.Telefono)
            .HasMaxLength(50);

        builder.Property(x => x.Email)
            .HasMaxLength(150);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();
    }
}