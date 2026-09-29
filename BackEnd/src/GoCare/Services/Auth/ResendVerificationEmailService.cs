using GoCare.Abstractions;
using GoCare.Data;
using GoCare.Models.Auth;
using GoCare.Models.Enums;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GoCare.Services.Auth;

public sealed class ResendVerificationEmailService(
    GoCareDbContext db,
    TokenService tokenService,
    IEmailSender emailSender,
    IOptions<FrontendOptions> frontendOptions)
{
    private readonly FrontendOptions _frontend = frontendOptions.Value;

    public async Task ResendVerificationEmailAsync(string email, CancellationToken ct)
    {
        var account = await db.Accounts.SingleOrDefaultAsync(a => a.Email == email, ct);

        // Risposta neutra: non rivela se l'account esiste o è già verificato.
        if (account is null || account.Status is not EAccountStatus.Unverified)
            return;

        var now = DateTimeOffset.UtcNow;

        var activeTokens = await db.EmailVerificationTokens
            .Where(t =>
                t.AccountId == account.Id &&
                t.ConsumedAt == null &&
                t.ExpiresAt > now)
            .ToListAsync(ct);

        foreach (var activeToken in activeTokens)
            activeToken.Consume(now);

        var newToken = new EmailVerificationToken(
            Guid.NewGuid(),
            account.Id,
            tokenService.GenerateRefreshTokenValue(),
            now.AddHours(24));

        db.EmailVerificationTokens.Add(newToken);
        await db.SaveChangesAsync(ct);

        await emailSender.SendAsync(
            account.Email,
            "Verifica il tuo account GoCare",
            $"<a href=\"{_frontend.VerifyEmailUrl}?token={newToken.Token}\">Clicca qui per verificare il tuo account</a>",
            ct);
    }
}
