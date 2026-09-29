using GoCare.Data;
using GoCare.Errors;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Auth;

public sealed class LoginService(
    GoCareDbContext db,
    PasswordService passwordService,
    TokenIssuer tokenIssuer)
{
    public async Task<(string accessToken, string refreshToken)> LoginAsync(
        string email, string password, CancellationToken ct)
    {
        var account = await db.Accounts.SingleOrDefaultAsync(a => a.Email == email, ct)
            ?? throw new ForbiddenException("Credenziali non valide.");

        if (!passwordService.Verify(password, account.PasswordHash))
            throw new ForbiddenException("Credenziali non valide");

        if (!account.CanLogIn)
            throw new ForbiddenException("Account non attivo");

        return await tokenIssuer.IssueAsync(account, DateTimeOffset.UtcNow, ct);
    }
}
