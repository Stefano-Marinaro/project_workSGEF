using GoCare.Data;
using GoCare.Errors;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Auth;

public sealed class RefreshService(
    GoCareDbContext db,
    TokenIssuer tokenIssuer)
{
    public async Task<(string accessToken, string refreshToken)> RefreshAsync(string refreshToken, CancellationToken ct)
    {
        var token = await db.RefreshTokens.SingleOrDefaultAsync(t => t.Token == refreshToken, ct)
            ?? throw new ForbiddenException("Refresh token non valido.");

        var now = DateTimeOffset.UtcNow;

        if (!token.IsActive(now))
            throw new ForbiddenException("Refresh token non valido o scaduto.");

        token.Revoke(now); // rotazione: questo token non sarà più riusabile

        var account = await db.Accounts.SingleOrDefaultAsync(a => a.Id == token.AccountId, ct)
            ?? throw new NotFoundException("Account non trovato.");

        if (!account.CanLogIn)
            throw new ForbiddenException("Account non attivo.");

        return await tokenIssuer.IssueAsync(account, now, ct);
    }
}
