using GoCare.Data;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Auth;

public sealed class LogoutService(GoCareDbContext db)
{
    public async Task LogoutAsync(string refreshToken, CancellationToken ct)
    {
        var token = await db.RefreshTokens.SingleOrDefaultAsync(t => t.Token == refreshToken, ct);
        if (token is null)
            return; // già invalido o sconosciuto: obiettivo già raggiunto, nessun errore

        token.Revoke(DateTimeOffset.UtcNow);
        await db.SaveChangesAsync(ct);
    }
}
