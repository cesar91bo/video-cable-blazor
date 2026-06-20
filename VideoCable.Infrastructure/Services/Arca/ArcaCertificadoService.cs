using System.Security.Cryptography.X509Certificates;
using Microsoft.EntityFrameworkCore;
using VideoCable.Domain.Enums;
using VideoCable.Infrastructure.Data;

namespace VideoCable.Infrastructure.Services.Arca;

public class ArcaCertificadoService
{
    private readonly AppDbContext _dbContext;

    public ArcaCertificadoService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string> ProbarCertificadoAsync(int empresaId, AmbienteFiscal ambiente)
    {
        var certificadoEmpresa = await _dbContext.CertificadosEmpresa
            .AsNoTracking()
            .Where(x =>
                x.EmpresaId == empresaId &&
                x.Ambiente == ambiente &&
                x.Activo &&
                !x.IsDeleted)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync();

        if (certificadoEmpresa is null)
            return "No se encontró un certificado activo para la empresa y ambiente.";

        if (string.IsNullOrWhiteSpace(certificadoEmpresa.RutaArchivo))
            return "El certificado no tiene ruta configurada.";

        if (!File.Exists(certificadoEmpresa.RutaArchivo))
            return $"No existe el archivo: {certificadoEmpresa.RutaArchivo}";

        try
        {
            var certificado = new X509Certificate2(
                certificadoEmpresa.RutaArchivo,
                certificadoEmpresa.Password,
                X509KeyStorageFlags.MachineKeySet |
                X509KeyStorageFlags.PersistKeySet |
                X509KeyStorageFlags.Exportable);

            return
                "Certificado cargado correctamente." + Environment.NewLine +
                $"Subject: {certificado.Subject}" + Environment.NewLine +
                $"Issuer: {certificado.Issuer}" + Environment.NewLine +
                $"Serial: {certificado.SerialNumber}" + Environment.NewLine +
                $"Válido desde: {certificado.NotBefore:dd/MM/yyyy HH:mm}" + Environment.NewLine +
                $"Válido hasta: {certificado.NotAfter:dd/MM/yyyy HH:mm}" + Environment.NewLine +
                $"Tiene clave privada: {certificado.HasPrivateKey}";
        }
        catch (Exception ex)
        {
            return $"Error al cargar certificado: {ex.Message}";
        }
    }
}