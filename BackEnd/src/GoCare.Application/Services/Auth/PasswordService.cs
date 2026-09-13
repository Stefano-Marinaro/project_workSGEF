using GoCare.Application.Models.Auth;

using Microsoft.AspNetCore.Identity;

namespace GoCare.Application.Services.Auth;

public sealed class PasswordService : IPasswordService
{
    private readonly PasswordHasher<Account> _hasher = new();

    public string Hash(string plainPassword) =>
        _hasher.HashPassword(user: null!, password: plainPassword);

    public bool Verify(string plainPassword, string hash)
    {
        var result = _hasher.VerifyHashedPassword(
            user: null!,
            hashedPassword: hash,
            providedPassword: plainPassword);

        return result is PasswordVerificationResult.Success
            or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
