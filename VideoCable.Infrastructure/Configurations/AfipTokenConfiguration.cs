using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoCable.Domain.Entities;

namespace VideoCable.Infrastructure.Configurations;

public class AfipTokenConfiguration : IEntityTypeConfiguration<AfipToken>
{
    public void Configure(EntityTypeBuilder<AfipToken> builder)
    {
        builder.ToTable("AfipTokens");

        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Empresa)
            .WithMany()
            .HasForeignKey(x => x.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Servicio)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Token)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.Sign)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(x => new 
        { 
            x.EmpresaId, 
            x.Ambiente, 
            x.Servicio, 
            x.FechaHasta 
        });
    }
}