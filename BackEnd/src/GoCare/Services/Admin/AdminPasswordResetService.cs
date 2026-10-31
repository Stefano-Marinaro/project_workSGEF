using GoCare.Data;
using GoCare.Errors;
using GoCare.Models.Enums;
using GoCare.Services.Auth;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Admin;

// l'admin fa partire il reset della password di un caregiver e chiude le sue sessioni (UC 11.5 · AD-05)
public sealed class AdminPasswordResetService(GoCareDbContext db, ForgotPasswordService forgotPassword)
{
    public async Task ResetAsync(Guid caregiverId, CancellationToken ct)
    {
        var account = await db.Accounts
            .SingleOrDefaultAsync(a => a.Id == caregiverId && a.Role == EAccountRole.Person, ct)
            ?? throw new NotFoundException("Caregiver non trovato.");

        if (account.Status is EAccountStatus.Deleted)
            throw new ConflictException("L'account è eliminato: va prima ripristinato.");

        // se qualcuno sta usando l'account al posto del caregiver, viene buttato fuori subito
        var now = DateTimeOffset.UtcNow;
        var sessions = await db.RefreshTokens
            .Where(t => t.AccountId == caregiverId && t.RevokedAt == null)
            .ToListAsync(ct);
        foreach (var session in sessions)
            session.Revoke(now);

        await db.SaveChangesAsync(ct);

        // stessa email del "password dimenticata": link con token monouso valido un'ora
        await forgotPassword.ForgotPasswordAsync(account.Email, ct);
    }
}
