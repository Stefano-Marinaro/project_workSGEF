namespace GoCare.Models.Domain;

public sealed class CaregiverAssistedLink
{
    private CaregiverAssistedLink() { } // costruttore vuoto: EF (materializzazione dal DB)

    public CaregiverAssistedLink(Guid caregiverId, Guid assistedPersonId, DateTimeOffset createdAt)
    {
        CaregiverId = caregiverId;
        AssistedPersonId = assistedPersonId;
        CreatedAt = createdAt;
    }

    // chiave composta (CaregiverId, AssistedPersonId): nessun Id surrogato, stesso pattern di CareGroupMembership
    public Guid CaregiverId { get; }        // Person.Id
    public Guid AssistedPersonId { get; }   // AssistedPerson.Id
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? DeletedAt { get; private set; }

    // v0: collegamento sempre diretto e subito attivo (nessuna accettazione richiesta, anche per un
    // secondo caregiver) — il flusso di invito con consenso è rinviato alla v1 (vedi Project_GoCare.md).
    public void Revoke(DateTimeOffset at) => DeletedAt ??= at;
}
