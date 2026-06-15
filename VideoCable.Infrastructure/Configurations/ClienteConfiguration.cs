using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoCable.Domain.Entities;

namespace VideoCable.Infrastructure.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Codigo)
            .IsRequired();

        builder.HasIndex(x => x.Codigo)
            .IsUnique();

        builder.Property(x => x.Nombre)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.NumeroDocumento)
            .HasMaxLength(20);

        builder.Property(x => x.Cuit)
            .HasMaxLength(11);

        builder.HasIndex(x => x.Cuit)
            .IsUnique()
            .HasFilter("[Cuit] IS NOT NULL");

        builder.HasIndex(x => new { x.TipoDocumento, x.NumeroDocumento })
            .IsUnique()
            .HasFilter("[NumeroDocumento] IS NOT NULL");

        builder.HasIndex(x => x.Nombre);

        builder.Property(x => x.Direccion)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(x => x.Localidad)
            .HasMaxLength(100);

        builder.Property(x => x.Telefono)
            .HasMaxLength(50);

        builder.Property(x => x.Email)
            .HasMaxLength(150);

        builder.Property(x => x.Observaciones)
            .HasMaxLength(1000);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();
    }
}