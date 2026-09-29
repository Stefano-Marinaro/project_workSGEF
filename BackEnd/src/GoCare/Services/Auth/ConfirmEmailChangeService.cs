using GoCare.Data;
using GoCare.Errors;
using GoCare.Models.Enums;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Auth;

public sealed class ConfirmEmailChangeService(GoCareDbContext db)
{
    public async Task ConfirmEmailChangeAsync(string tokenValue, CancellationToken ct)
    {
        var token = await db.EmailChangeTokens.SingleOrDefaultAsync(t => t.Token == tokenValue, ct)
            ?? throw new NotFoundException("Token di cambio e-mail non trovato.");

        var now = DateTimeOffset.UtcNow;

        if (!token.IsUsable(now))
            throw new ForbiddenException("Token di cambio e-mail non valido o scaduto.");

        var account = await db.Accounts.SingleOrDefaultAsync(a => a.Id == token.AccountId, ct)
            ?? throw new NotFoundException("Account non trovato.");

        if (!account.CanLogIn)
            throw new ForbiddenException("Account non attivo.");

        var emailAlreadyUsed = await db.Accounts.AnyAsync(
            a => a.Email == token.NewEmail && a.Id != account.Id,
            ct);

        if (emailAlreadyUsed)
            throw new ConflictException("E-mail già registrata.");

        account.ChangeEmail(token.NewEmail, now);

        if (account.Role is EAccountRole.Person)
        {
            var person = await db.Persons.SingleOrDefaultAsync(p => p.Id == account.Id, ct)
                ?? throw new NotFoundException("Profilo caregiver non trovato.");

            person.ChangeEmail(token.NewEmail);
        }
        else if (account.Role is EAccountRole.Association)
        {
            var association = await db.Associations.SingleOrDefaultAsync(a => a.Id == account.Id, ct)
                ?? throw new NotFoundException("Profilo associazione non trovato.");

            association.ChangeEmail(token.NewEmail);
        }

        token.Consume(now);

        var activeSessions = await db.RefreshTokens
            .Where(t => t.AccountId == account.Id && t.RevokedAt == null)
            .ToListAsync(ct);

        foreach (var activeSession in activeSessions)
            activeSession.Revoke(now);

        await db.SaveChangesAsync(ct);
    }
}
