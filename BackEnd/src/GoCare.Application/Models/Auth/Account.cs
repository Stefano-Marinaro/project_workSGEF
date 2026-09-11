using GoCare.Application.Models.Enums;

namespace GoCare.Application.Models.Auth;

public sealed class Account
{
    private Account() { }

    public Account(
        Guid id,
        string email,
        string passwordHash,
        EAccountRole role,
        DateTimeOffset createdAt)
    {
        Id = id;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        CreatedAt = createdAt;
    }

    public Guid Id { get; }
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public EAccountRole Role { get; } 
    public EAccountStatus Status { get; private set; } = EAccountStatus.Unverified;
    public DateTimeOffset CreatedAt { get; } 
    public DateTimeOffset? EmailVerifiedAt { get; private set; }
    public DateTimeOffset? SuspendedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

}

      
