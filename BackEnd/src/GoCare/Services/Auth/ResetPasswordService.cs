using GoCare.Data;
using GoCare.Errors;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Auth;

public sealed class ResetPasswordService(
    GoCareDbContext db,
    PasswordService passwordService)
{
    public async Task ResetPasswordAsync(string token, string newPassword, CancellationToken ct)
    {
        var resetToken = await db.PasswordResetTokens.SingleOrDefaultAsync(t => t.Token == token, ct)
            ?? throw new NotFoundException("Token di reset non trovato.");

        var now = DateTimeOffset.UtcNow;

        if (!resetToken.IsUsable(now))
            throw new ForbiddenException("Token di reset non valido o scaduto.");

        resetToken.Consume(now);

        var account = await db.Accounts.SingleOrDefaultAsync(a => a.Id == resetToken.AccountId, ct)
            ?? throw new NotFoundException("Account non trovato.");

        account.ChangePassword(passwordService.Hash(newPassword));

        // password cambiata: tutte le sessioni attive di questo account vengono buttate fuori
        var activeSessions = await db.RefreshTokens
            .Where(t => t.AccountId == account.Id && t.RevokedAt == null)
            .ToListAsync(ct);

        foreach (var session in activeSessions)
            session.Revoke(now);

        await db.SaveChangesAsync(ct);
    }
}
