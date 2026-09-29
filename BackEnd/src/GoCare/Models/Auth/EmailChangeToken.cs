namespace GoCare.Models.Auth;

public sealed class EmailChangeToken : IExpirable
{
    private EmailChangeToken() { }

    public EmailChangeToken(
        Guid id,
        Guid accountId,
        string newEmail,
        string token,
        DateTimeOffset expiresAt)
    {
        Id = id;
        AccountId = accountId;
        NewEmail = newEmail;
        Token = token;
        ExpiresAt = expiresAt;
    }

    public Guid Id { get; }
    public Guid AccountId { get; }
    public string NewEmail { get; } = null!;
    public string Token { get; } = null!;
    public DateTimeOffset ExpiresAt { get; }
    public DateTimeOffset? ConsumedAt { get; private set; }

    public bool IsUsable(DateTimeOffset now) =>
        ConsumedAt is null && !this.IsExpired(now);

    public void Consume(DateTimeOffset at)
    {
        if (!IsUsable(at))
            throw new InvalidOperationException("Il token non è più utilizzabile.");

        ConsumedAt = at;
    }
}
