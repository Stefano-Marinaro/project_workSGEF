using GoCare.Application.Models.Enums;

namespace GoCare.Application.Services.Auth;

public interface ITokenService
{
    string CreateAccessToken(
        Guid accountId,
        EAccountRole role,
        bool emailVerified,
        DateTimeOffset now);

    string GenerateRefreshTokenValue();
}
