using Microsoft.EntityFrameworkCore;
using VideoCable.Infrastructure.Data;

namespace VideoCable.Infrastructure.Services.Arca;

// Un lock de sesión de SQL Server coordina distintos usuarios y procesos del sistema.
public sealed class ArcaSqlLock(AppDbContext db, string recurso) : IAsyncDisposable
{
    public static async Task<ArcaSqlLock> Adquirir(AppDbContext db, string recurso)
    {
        await db.Database.OpenConnectionAsync();
        try
        {
            await using var command = db.Database.GetDbConnection().CreateCommand();
            command.CommandText = "DECLARE @r int; EXEC @r = sys.sp_getapplock @Resource=@recurso, @LockMode='Exclusive', @LockOwner='Session', @LockTimeout=10000; SELECT @r;";
            var p = command.CreateParameter(); p.ParameterName = "@recurso"; p.Value = recurso; command.Parameters.Add(p);
            var resultado = Convert.ToInt32(await command.ExecuteScalarAsync());
            if (resultado < 0) throw new InvalidOperationException("Hay otra operación ARCA en curso. Espere unos segundos y vuelva a intentar.");
            return new ArcaSqlLock(db, recurso);
        }
        catch { await db.Database.CloseConnectionAsync(); throw; }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await using var command = db.Database.GetDbConnection().CreateCommand();
            command.CommandText = "EXEC sys.sp_releaseapplock @Resource=@recurso, @LockOwner='Session';";
            var p = command.CreateParameter(); p.ParameterName = "@recurso"; p.Value = recurso; command.Parameters.Add(p);
            await command.ExecuteNonQueryAsync();
        }
        finally { await db.Database.CloseConnectionAsync(); }
    }
}
