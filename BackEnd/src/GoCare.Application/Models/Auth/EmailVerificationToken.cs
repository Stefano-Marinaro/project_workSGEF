namespace GoCare.Application.Models.Auth;

public sealed class EmailVerificationToken : IExpirable
{
    private EmailVerificationToken() { }

    public EmailVerificationToken(Guid id, Guid accountId, string token, DateTimeOffset expiresAt)
    {
        Id = id;
        AccountId = accountId;
        Token = token;
        ExpiresAt = expiresAt;
    }

    public Guid Id { get; }
    public Guid AccountId { get; }               
    public string Token { get; } = null!;
    public DateTimeOffset ExpiresAt { get; }
    public DateTimeOffset? ConsumedAt { get; private set; }

    public void Consume(DateTimeOffset at)
    {
        if (ConsumedAt is not null)
            throw new InvalidOperationException("Il token è già stato utilizzato.");
        if (this.IsExpired(at))
            throw new InvalidOperationException("Il token è scaduto.");

        ConsumedAt = at;
    }

    public bool IsUsable(DateTimeOffset now) => ConsumedAt is null && !this.IsExpired(now);
}
