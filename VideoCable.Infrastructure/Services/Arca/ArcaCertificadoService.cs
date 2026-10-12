using Microsoft.EntityFrameworkCore;
using VideoCable.Domain.Enums;
using VideoCable.Infrastructure.Data;
namespace VideoCable.Infrastructure.Services.Arca;

public sealed class ArcaCertificadoService(IDbContextFactory<AppDbContext> factory, ArcaSecretos secretos)
{
    public async Task<string> ProbarCertificadoAsync(int empresaId, AmbienteFiscal ambiente)
    {
        await using var db = await factory.CreateDbContextAsync();
        var certificado = await db.CertificadosEmpresa.AsNoTracking()
            .Where(x => x.EmpresaId == empresaId && x.Ambiente == ambiente && x.Activo && !x.IsDeleted)
            .OrderByDescending(x => x.Id).FirstOrDefaultAsync();
        if (certificado is null) return $"No hay certificado activo de {ambiente}.";
        using var x509 = secretos.Abrir(certificado);
        return $"Certificado vigente con clave privada para {ambiente}. Vence el {x509.NotAfter:dd/MM/yyyy}.";
    }
}
