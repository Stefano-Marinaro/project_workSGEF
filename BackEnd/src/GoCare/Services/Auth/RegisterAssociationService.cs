using GoCare.Abstractions;
using GoCare.Data;
using GoCare.Errors;
using GoCare.Models.Auth;
using GoCare.Models.Enums;
using GoCare.Services.Provisioning;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GoCare.Services.Auth;

public sealed class RegisterAssociationService(
    GoCareDbContext db,
    PasswordService passwordService,
    TokenService tokenService,
    ProfileProvisioningService provisioning,
    IEmailSender emailSender,
    IOptions<FrontendOptions> frontendOptions)
{
    private readonly FrontendOptions _frontend = frontendOptions.Value;

    public async Task<Guid> RegisterAssociationAsync(string email, string password, CancellationToken ct)
    {
        var emailExist = await db.Accounts.AnyAsync(a => a.Email == email, ct);
        if (emailExist)
            throw new ConflictException("Email già registrata");

        var accountId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var account = new Account(accountId, email, passwordService.Hash(password), EAccountRole.Association, now);

        var verificationToken = new EmailVerificationToken(
           Guid.NewGuid(), accountId, tokenService.GenerateRefreshTokenValue(), expiresAt: now.AddHours(0.5));

        db.Accounts.Add(account);
        db.EmailVerificationTokens.Add(verificationToken);

        await db.SaveChangesAsync(ct);

        await provisioning.CreateAssociationSkeletonAsync(accountId, email, ct);

        await emailSender.SendAsync(
            email, "Verifica il tuo account GoCare",
            $"<a href=\"{_frontend.VerifyEmailUrl}?token={verificationToken.Token}\">Clicca qui per verificare il tuo account</a>", ct);

        return accountId;
    }
}
