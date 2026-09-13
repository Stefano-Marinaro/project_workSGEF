using GoCare.Application.Data;              
using GoCare.Application.Models.Auth;       
using GoCare.Shared.Abstractions;           
using GoCare.Shared.Errors;               
using Microsoft.EntityFrameworkCore;       
using Microsoft.Extensions.Options;

namespace GoCare.Application.Services.Auth;

public sealed class AuthService(
    AuthDbContext db,                   // lettura account e salvataggio RefreshToken
    IPasswordService passwordService,   // verificare la password
    ITokenService tokenService,         // servizi token
    IClock clock,                       // per i test, 
    IOptions<JwtOptions> jwtOptions ) : IAuthService   // per RefreshTokenDays
{
    private readonly JwtOptions _jwt = jwtOptions.Value;

    public async Task<(string accessToken, string refreshToken)> LoginAsync(
        string email, string password, CancellationToken ct)
    {
        var account = await db.Accounts.SingleOrDefaultAsync(a => a.Email == email, ct)
            ?? throw new ForbiddenException("Credenziali non valide.");

        if (!passwordService.Verify(password, account.PasswordHash))
            throw new ForbiddenException("Credenziali non valide");

        if (!account.CanLogIn)
            throw new ForbiddenException("Account non attivo");

        return await IssueTokens(account, clock.UtcNow, ct );
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
}
