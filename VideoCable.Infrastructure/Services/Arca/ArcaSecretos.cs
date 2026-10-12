using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using VideoCable.Infrastructure.Data;

namespace VideoCable.Infrastructure.Services.Arca;

public sealed class ArcaSecretos(IDataProtectionProvider provider, IDbContextFactory<AppDbContext> factory)
{
    private readonly IDataProtector protector = provider.CreateProtector("VideoCable.ARCA.Certificados.v1");
    private const string Prefijo = "dp:v1:";
    public string Proteger(string texto) => Prefijo + protector.Protect(texto);
    public string Leer(string? texto) => texto?.StartsWith(Prefijo, StringComparison.Ordinal) == true
        ? protector.Unprotect(texto[Prefijo.Length..]) : texto ?? "";

    public X509Certificate2 Abrir(CertificadoEmpresa certificado)
    {
        var bytes = certificado.ArchivoPfxProtegido is not null
            ? protector.Unprotect(certificado.ArchivoPfxProtegido)
            : File.ReadAllBytes(certificado.RutaArchivo ?? throw new InvalidOperationException("El certificado no tiene archivo."));
        var x509 = X509CertificateLoader.LoadPkcs12(bytes, Leer(certificado.Password), X509KeyStorageFlags.EphemeralKeySet);
        if (!x509.HasPrivateKey || x509.NotBefore.ToUniversalTime() > DateTime.UtcNow || x509.NotAfter.ToUniversalTime() <= DateTime.UtcNow)
        {
            x509.Dispose();
            throw new InvalidOperationException("El certificado no tiene clave privada o está fuera de su período de validez.");
        }
        return x509;
    }

    public async Task MigrarLegadosAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        var certificados = await db.CertificadosEmpresa.Where(x => !x.IsDeleted).ToListAsync();
        foreach (var certificado in certificados)
        {
            if (certificado.Password?.StartsWith(Prefijo, StringComparison.Ordinal) != true)
                certificado.Password = Proteger(certificado.Password ?? "");
            if (certificado.ArchivoPfxProtegido is null && File.Exists(certificado.RutaArchivo))
                certificado.ArchivoPfxProtegido = protector.Protect(await File.ReadAllBytesAsync(certificado.RutaArchivo!));
        }
        var tokens = await db.AfipTokens.ToListAsync();
        foreach (var token in tokens)
        {
            if (!token.Token.StartsWith(Prefijo, StringComparison.Ordinal)) token.Token = Proteger(token.Token);
            if (!token.Sign.StartsWith(Prefijo, StringComparison.Ordinal)) token.Sign = Proteger(token.Sign);
        }
        await db.SaveChangesAsync();
    }

    public async Task<string> GuardarCertificadoAsync(int empresaId, VideoCable.Domain.Enums.AmbienteFiscal ambiente, byte[] pfx, string password)
    {
        using var x509 = X509CertificateLoader.LoadPkcs12(pfx, password, X509KeyStorageFlags.EphemeralKeySet);
        if (!x509.HasPrivateKey || x509.NotBefore.ToUniversalTime() > DateTime.UtcNow || x509.NotAfter.ToUniversalTime() <= DateTime.UtcNow)
            throw new InvalidOperationException("El PFX debe tener clave privada y estar vigente.");
        await using var db = await factory.CreateDbContextAsync();
        await using var bloqueo = await ArcaSqlLock.Adquirir(db, $"ARCA-CERT:{empresaId}:{ambiente}");
        var anteriores = await db.CertificadosEmpresa.Where(x => x.EmpresaId == empresaId && x.Ambiente == ambiente).ToListAsync();
        foreach (var anterior in anteriores) anterior.Activo = false;
        var certificado = anteriores.FirstOrDefault(x => x.SerialNumber == x509.SerialNumber);
        if (certificado is null) { certificado = new CertificadoEmpresa { EmpresaId = empresaId, Ambiente = ambiente, CreatedAt = DateTime.Now }; db.CertificadosEmpresa.Add(certificado); }
        certificado.Password = Proteger(password); certificado.ArchivoPfxProtegido = protector.Protect(pfx);
        certificado.SerialNumber = x509.SerialNumber; certificado.Subject = x509.Subject;
        certificado.ValidFrom = x509.NotBefore; certificado.ValidTo = x509.NotAfter;
        certificado.StoreLocation = "Protegido"; certificado.StoreName = "Base de datos";
        certificado.RutaArchivo = null; certificado.Activo = true; certificado.IsDeleted = false;
        var tokens = await db.AfipTokens.Where(x => x.EmpresaId == empresaId && x.Ambiente == ambiente).ToListAsync();
        db.AfipTokens.RemoveRange(tokens);
        await db.SaveChangesAsync();
        return $"Certificado guardado para {ambiente}. Vence el {x509.NotAfter:dd/MM/yyyy}.";
    }
}
