namespace GoCare.Application.Services.Auth;
public interface IAuthService
{
    Task<(string accessToken, string refreshToken)> LoginAsync(string email, string password, CancellationToken ct);
}
