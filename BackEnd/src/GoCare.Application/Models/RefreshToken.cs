namespace GoCare.Application.Models;

/// <summary>Sessione rinnovabile di un account; può essere revocata prima della scadenza.</summary>
public sealed class RefreshToken : Entity, ISoftDeletable
{
    public Guid AccountId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public string? UserAgent { get; set; }
    public string? IpAddress { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
