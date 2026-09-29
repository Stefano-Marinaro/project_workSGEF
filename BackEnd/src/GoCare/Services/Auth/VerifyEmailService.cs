using GoCare.Data;
using GoCare.Errors;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Auth;

public sealed class VerifyEmailService(GoCareDbContext db)
{
    public async Task VerifyAccount(string token, CancellationToken ct)
    {
        var verificationToken = await db.EmailVerificationTokens.SingleOrDefaultAsync(t => t.Token == token, ct)
            ?? throw new NotFoundException("Token di verifica non trovato.");

        var now = DateTimeOffset.UtcNow;

        if (!verificationToken.IsUsable(now))
            throw new ForbiddenException("Token di verifica non valido o scaduto.");

        verificationToken.Consume(now);

        var account = await db.Accounts.SingleOrDefaultAsync(a => a.Id == verificationToken.AccountId, ct)
            ?? throw new NotFoundException("Account non trovato.");

        account.Verify(now);

        await db.SaveChangesAsync(ct);
    }
}
