using GoCare.Data;              
using GoCare.Models.Auth;
using GoCare.Models.Enums;
using GoCare.Services.Provisioning;
using GoCare.Abstractions;           
using GoCare.Errors;               

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GoCare.Services.Auth;

public sealed class AuthService(
    GoCareDbContext db,                 // lettura account e salvataggio RefreshToken
    PasswordService passwordService,    // verificare la password
    TokenService tokenService,          // servizi token
    ProfileProvisioningService provisioning,
    IEmailSender emailSender,
    IOptions<JwtOptions> jwtOptions,   // per RefreshTokenDays
    IOptions<FrontendOptions> frontendOptions )
{
    private readonly JwtOptions _jwt = jwtOptions.Value;
    private readonly FrontendOptions _frontend = frontendOptions.Value;

    public async Task<(string accessToken, string refreshToken)> LoginAsync(
        string email, string password, CancellationToken ct)
    {
        var account = await db.Accounts.SingleOrDefaultAsync(a => a.Email == email, ct)
            ?? throw new ForbiddenException("Credenziali non valide.");

        if (!passwordService.Verify(password, account.PasswordHash))
            throw new ForbiddenException("Credenziali non valide");

        if (!account.CanLogIn)
            throw new ForbiddenException("Account non attivo");

        return await IssueTokens(account, DateTimeOffset.UtcNow, ct );
    }

    public async Task<Guid> RegisterUserAsync(string email, string password, CancellationToken ct)
    {
        var emailExist = await db.Accounts.AnyAsync(a => a.Email == email, ct);
        if (emailExist)
            throw new ConflictException("Email già registrata");

        var accountId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var account = new Account(accountId, email, passwordService.Hash(password), EAccountRole.Person, now);

        var verificationToken = new EmailVerificationToken(
           Guid.NewGuid(), accountId, tokenService.GenerateRefreshTokenValue(), expiresAt: now.AddHours(24));

        db.Accounts.Add(account);
        db.EmailVerificationTokens.Add(verificationToken);

        await db.SaveChangesAsync(ct);

        await provisioning.CreatePersonSkeletonAsync(accountId, email, ct);

        await emailSender.SendAsync(
            email, "Verifica il tuo account GoCare",
            $"<a href=\"{_frontend.VerifyEmailUrl}?token={verificationToken.Token}\">Clicca qui per verificare il tuo account</a>", ct);

        return accountId;
    }

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

    private async Task<(string accessToken, string refreshToken)> IssueTokens(Account account, DateTimeOffset now, CancellationToken ct)
    {
        var accessToken = tokenService.CreateAccessToken(
            account.Id, account.Role, account.EmailVerifiedAt is not null, now);

        var refreshValue = tokenService.GenerateRefreshTokenValue(); // sarà il valore ricercato dalle funzioni di controllo del token

        var refreshToken = new RefreshToken(
            Guid.NewGuid(), account.Id, refreshValue, expiresAt: now.AddDays(_jwt.RefreshTokenDays), createdAt: now);

        db.RefreshTokens.Add(refreshToken);
        await db.SaveChangesAsync(ct);

        return (accessToken, refreshValue);
    }


    public async Task VerifyAccount( string token, CancellationToken ct)
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

    public async Task ForgotPasswordAsync( string email, CancellationToken ct)
    {
        var account = await db.Accounts.SingleOrDefaultAsync(a => a.Email == email, ct);
        if (account is null)
            return;

        var now = DateTimeOffset.UtcNow;

        var resetToken = new PasswordResetToken(
            Guid.NewGuid(), account.Id, tokenService.GenerateRefreshTokenValue(), expiresAt: now.AddHours(1));

        db.PasswordResetTokens.Add(resetToken);
        await db.SaveChangesAsync(ct);

        await emailSender.SendAsync(
            email, "Reimposta la password di GoCare",
            $"<a href=\"{_frontend.ResetPasswordUrl}?token={resetToken.Token}\">Clicca qui per reimpostare la password</a>", ct);
    }

    public async Task ResetPasswordAsync( string token, string newPassword, CancellationToken ct)
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

        var activeSessions = await db.RefreshTokens
            .Where(t => t.AccountId == account.Id && t.RevokedAt == null)
            .ToListAsync(ct);

        foreach (var session in activeSessions)
            session.Revoke(now);

        await db.SaveChangesAsync(ct);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken ct)
    {
        var token = await db.RefreshTokens.SingleOrDefaultAsync(t => t.Token == refreshToken, ct);
        if (token is null)
            return; // già invalido o sconosciuto: obiettivo già raggiunto, nessun errore

        token.Revoke(DateTimeOffset.UtcNow);
        await db.SaveChangesAsync(ct);
    }

    public async Task<(string accessToken, string refreshToken)> RefreshAsync(string refreshToken, CancellationToken ct)
    {
        var token = await db.RefreshTokens.SingleOrDefaultAsync(t => t.Token == refreshToken, ct)
            ?? throw new ForbiddenException("Refresh token non valido.");

        var now = DateTimeOffset.UtcNow;

        if (!token.IsActive(now))
            throw new ForbiddenException("Refresh token non valido o scaduto.");

        token.Revoke(now); // rotazione: questo token non sarà più riusabile

        var account = await db.Accounts.SingleOrDefaultAsync(a => a.Id == token.AccountId, ct)
            ?? throw new NotFoundException("Account non trovato.");

        if (!account.CanLogIn)
            throw new ForbiddenException("Account non attivo.");

        return await IssueTokens(account, now, ct);
    }
}
