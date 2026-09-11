namespace GoCare.Application.Models.Auth;

public sealed class RefreshToken : IExpirable
{
    private RefreshToken() { }

    public RefreshToken(Guid id, Guid accountId, string token, DateTimeOffset expiresAt, DateTimeOffset createdAt)
    {
        Id = id;
        AccountId = accountId;
        Token = token;
        ExpiresAt = expiresAt;
        CreatedAt = createdAt;
    }

    public Guid Id { get; }
    public Guid AccountId { get; }
    public string Token { get; } = null!;
    public DateTimeOffset ExpiresAt { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? RevokedAt { get; private set; }

    public void Revoke(DateTimeOffset at) => RevokedAt ??= at;

    public bool IsActive(DateTimeOffset now) => RevokedAt is null && !this.IsExpired(now);
}
