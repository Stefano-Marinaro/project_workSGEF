using GoCare.Models.Auth; // IExpirable + estensione IsExpired

namespace GoCare.Models.Domain;

public sealed class CaregiverInviteLink : IExpirable
{
    private CaregiverInviteLink() { } // costruttore vuoto: EF (materializzazione dal DB)

    public CaregiverInviteLink(
        Guid id, Guid assistedPersonId, Guid invitedByPersonId, string token, DateTimeOffset expiresAt, DateTimeOffset createdAt)
    {
        Id = id;
        AssistedPersonId = assistedPersonId;
        InvitedByPersonId = invitedByPersonId;
        Token = token;
        ExpiresAt = expiresAt;
        CreatedAt = createdAt;
    }

    public Guid Id { get; }
    public Guid AssistedPersonId { get; }
    public Guid InvitedByPersonId { get; }  // Person.Id di chi ha generato il link, per mostrare "da chi arriva l'invito"
    public string Token { get; } = null!;
    public DateTimeOffset ExpiresAt { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? RevokedAt { get; private set; }

    public bool IsUsable(DateTimeOffset now) => RevokedAt is null && !this.IsExpired(now);

    public void Revoke(DateTimeOffset at) => RevokedAt ??= at;
}
