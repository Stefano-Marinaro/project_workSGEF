using GoCare.Data;
using GoCare.Models.Auth;

using Microsoft.Extensions.Options;

namespace GoCare.Services.Auth;

// Emette la coppia access/refresh token: condiviso da LoginService e RefreshService.
public sealed class TokenIssuer(
    GoCareDbContext db,
    TokenService tokenService,
    IOptions<JwtOptions> jwtOptions)
{
    private readonly JwtOptions _jwt = jwtOptions.Value;

    public async Task<(string accessToken, string refreshToken)> IssueAsync(
        Account account, DateTimeOffset now, CancellationToken ct)
    {
        var accessToken = tokenService.CreateAccessToken(
            account.Id, account.Role, account.EmailVerifiedAt is not null, now);

        var refreshValue = tokenService.GenerateRefreshTokenValue();

        var refreshToken = new RefreshToken(
            Guid.NewGuid(), account.Id, refreshValue, expiresAt: now.AddDays(_jwt.RefreshTokenDays), createdAt: now);

        db.RefreshTokens.Add(refreshToken);
        await db.SaveChangesAsync(ct);

        return (accessToken, refreshValue);
    }
}
