using GoCare.Data;
using GoCare.Errors;
using GoCare.Models.Enums;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Auth;

// L'associazione invitata dall'admin sceglie la password (UC 11.1 · AD-01).
// Come il reset password, ma in più attiva l'account: il link è arrivato alla sua email, quindi l'email è verificata.
public sealed class SetPasswordService(
    GoCareDbContext db,
    PasswordService passwordService)
{
    public async Task SetPasswordAsync(string token, string password, CancellationToken ct)
    {
        var inviteToken = await db.PasswordResetTokens.SingleOrDefaultAsync(t => t.Token == token, ct)
            ?? throw new NotFoundException("Token di invito non trovato.");

        var now = DateTimeOffset.UtcNow;

        if (!inviteToken.IsUsable(now))
            throw new ForbiddenException("Token di invito non valido o scaduto.");

        var account = await db.Accounts.SingleOrDefaultAsync(a => a.Id == inviteToken.AccountId, ct)
            ?? throw new NotFoundException("Account non trovato.");

        if (account.Status is EAccountStatus.Deleted)
            throw new ForbiddenException("L'account non esiste più.");

        inviteToken.Consume(now);
        account.ChangePassword(passwordService.Hash(password));

        if (account.Status is EAccountStatus.Unverified)
            account.Verify(now);

        // eventuali sessioni aperte con la vecchia password non valgono più
        var activeSessions = await db.RefreshTokens
            .Where(t => t.AccountId == account.Id && t.RevokedAt == null)
            .ToListAsync(ct);

        foreach (var session in activeSessions)
            session.Revoke(now);

        await db.SaveChangesAsync(ct);
    }
}
