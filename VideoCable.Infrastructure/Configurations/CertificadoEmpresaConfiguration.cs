using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoCable.Domain.Entities;

namespace VideoCable.Infrastructure.Configurations;

public class CertificadoEmpresaConfiguration : IEntityTypeConfiguration<CertificadoEmpresa>
{
    public void Configure(EntityTypeBuilder<CertificadoEmpresa> builder)
    {
        builder.ToTable("CertificadosEmpresa");

        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Empresa)
            .WithMany()
            .HasForeignKey(x => x.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.StoreLocation)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.StoreName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.SerialNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Subject)
            .HasMaxLength(300);

        builder.HasIndex(x => new { x.EmpresaId, x.Ambiente, x.Activo });

        builder.HasIndex(x => new { x.EmpresaId, x.Ambiente, x.SerialNumber })
            .IsUnique();

        builder.Property(x => x.RowVersion)
            .IsRowVersion();
    }
}