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
}
