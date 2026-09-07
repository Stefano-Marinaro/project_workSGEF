namespace GoCare.Application.Models;

/// <summary>Token monouso per il ripristino della password.</summary>
public sealed class PasswordResetToken : Entity, ISoftDeletable
{
    public Guid AccountId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? UsedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
