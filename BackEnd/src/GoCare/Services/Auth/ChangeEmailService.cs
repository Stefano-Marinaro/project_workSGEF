using GoCare.Abstractions;
using GoCare.Data;
using GoCare.Errors;
using GoCare.Models.Auth;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GoCare.Services.Auth;

public sealed class ChangeEmailService(
    GoCareDbContext db,
    PasswordService passwordService,
    TokenService tokenService,
    IEmailSender emailSender,
    IOptions<FrontendOptions> frontendOptions)
{
    private readonly FrontendOptions _frontend = frontendOptions.Value;

    public async Task RequestEmailChangeAsync(
        Guid accountId,
        string newEmail,
        string currentPassword,
        CancellationToken ct)
    {
        var account = await db.Accounts.SingleOrDefaultAsync(a => a.Id == accountId, ct)
            ?? throw new NotFoundException("Account non trovato.");

        if (!account.CanLogIn)
            throw new ForbiddenException("Account non attivo.");

        if (!passwordService.Verify(currentPassword, account.PasswordHash))
            throw new ForbiddenException("Password non valida.");

        if (string.Equals(account.Email, newEmail, StringComparison.OrdinalIgnoreCase))
            throw new ConflictException("La nuova e-mail coincide con quella attuale.");

        var emailAlreadyUsed = await db.Accounts.AnyAsync(
            a => a.Email == newEmail && a.Id != accountId,
            ct);

        if (emailAlreadyUsed)
            throw new ConflictException("E-mail già registrata.");

        var now = DateTimeOffset.UtcNow;

        var previousTokens = await db.EmailChangeTokens
            .Where(t =>
                t.AccountId == accountId &&
                t.ConsumedAt == null &&
                t.ExpiresAt > now)
            .ToListAsync(ct);

        foreach (var previousToken in previousTokens)
            previousToken.Consume(now);

        var token = new EmailChangeToken(
            Guid.NewGuid(),
            accountId,
            newEmail,
            tokenService.GenerateRefreshTokenValue(),
            now.AddHours(1));

        db.EmailChangeTokens.Add(token);
        await db.SaveChangesAsync(ct);

        await emailSender.SendAsync(
            newEmail,
            "Conferma la nuova e-mail GoCare",
            $"<a href=\"{_frontend.ChangeEmailUrl}?token={token.Token}\">Conferma il cambio di e-mail</a>",
            ct);
    }
}
