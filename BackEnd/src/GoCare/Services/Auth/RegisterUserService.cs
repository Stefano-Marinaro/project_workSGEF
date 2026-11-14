using GoCare.Infrastructure;
using GoCare.Abstractions;
using GoCare.Data;
using GoCare.Errors;
using GoCare.Models.Auth;
using GoCare.Models.Enums;
using GoCare.Models.Domain;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GoCare.Services.Auth;

public sealed class RegisterUserService(
    GoCareDbContext db,
    PasswordService passwordService,
    TokenService tokenService,
    IEmailSender emailSender,
    IOptions<FrontendOptions> frontendOptions)
{
    private readonly FrontendOptions _frontend = frontendOptions.Value;

    public async Task<Guid> RegisterUserAsync(string email, string password, string name, string surname, CancellationToken ct)
    {
        email = email.Trim().ToLowerInvariant();
        var emailExist = await db.Accounts.AnyAsync(a => a.Email.ToLower() == email, ct);
        if (emailExist)
            throw new ConflictException("Email già registrata");

        var accountId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var account = new Account(accountId, email, passwordService.Hash(password), EAccountRole.Person, now);

        var verificationToken = new EmailVerificationToken(
           Guid.NewGuid(), accountId, tokenService.GenerateRefreshTokenValue(), expiresAt: now.AddHours(24));

        db.Accounts.Add(account);
        db.EmailVerificationTokens.Add(verificationToken);
        db.Persons.Add(new Person(accountId, email, name.Trim(), surname.Trim()));

        // Account, token e identità vengono salvati insieme: il primo accesso ha già le iniziali.
        await db.SaveChangesAsync(ct);

        var (subject, html) = EmailTemplates.VerifyAccount(
            $"{_frontend.VerifyEmailUrl}?token={verificationToken.Token}", verificationToken.ExpiresAt - now);

        await emailSender.SendAsync(email, subject, html, ct);

        return accountId;
    }
}
