using GoCare.Infrastructure;
using GoCare.Abstractions;
using GoCare.Data;
using GoCare.Models.Auth;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GoCare.Services.Auth;

public sealed class ForgotPasswordService(
    GoCareDbContext db,
    TokenService tokenService,
    IEmailSender emailSender,
    IOptions<FrontendOptions> frontendOptions)
{
    private readonly FrontendOptions _frontend = frontendOptions.Value;

    public async Task ForgotPasswordAsync(string email, CancellationToken ct)
    {
        email = email.Trim().ToLowerInvariant();
        var account = await db.Accounts.SingleOrDefaultAsync(a => a.Email.ToLower() == email, ct);
        if (account is null)
            return; // stessa risposta sia che l'email esista o no: niente enumerazione utenti

        var now = DateTimeOffset.UtcNow;

        var resetToken = new PasswordResetToken(
            Guid.NewGuid(), account.Id, tokenService.GenerateRefreshTokenValue(), expiresAt: now.AddHours(1));

        db.PasswordResetTokens.Add(resetToken);
        await db.SaveChangesAsync(ct);

        var (subject, html) = EmailTemplates.ResetPassword(
            $"{_frontend.ResetPasswordUrl}?token={resetToken.Token}", resetToken.ExpiresAt - now);

        await emailSender.SendAsync(email, subject, html, ct);
    }
}
