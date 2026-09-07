namespace GoCare.Application.Models;

/// <summary>Un tentativo di accesso non riuscito, usato per rate limiting e blocchi temporanei.</summary>
public sealed class FailedLoginAttempt : Entity, ISoftDeletable
{
    public Guid? AccountId { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTimeOffset AttemptedAt { get; set; }
    public string? IpAddress { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
