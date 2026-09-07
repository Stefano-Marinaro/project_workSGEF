namespace GoCare.Application.Models;

/// <summary>Token monouso usato per verificare l'indirizzo e-mail di un account.</summary>
public sealed class EmailVerificationToken : Entity, ISoftDeletable
{
    public Guid AccountId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? UsedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
