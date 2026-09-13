using GoCare.Application.Models.Enums;

namespace GoCare.Application.Models.Domain;

public sealed class ContactAccessLog
{
    private ContactAccessLog() { } // costruttore vuoto: EF (materializzazione dal DB)

    public ContactAccessLog(Guid id, Guid transportRequestId, Guid associationId, EContactDataKind dataKind, DateTimeOffset accessedAt)
    {
        Id = id;
        TransportRequestId = transportRequestId;
        AssociationId = associationId;
        DataKind = dataKind;
        AccessedAt = accessedAt;
    }

    public Guid Id { get; }
    public Guid TransportRequestId { get; }
    public Guid AssociationId { get; }
    public EContactDataKind DataKind { get; }
    public DateTimeOffset AccessedAt { get; }
}
