using GoCare.Application.Models.Enums;

namespace GoCare.Application.Models.Domain;

public sealed class ContactAccessLog(Guid id, Guid transportRequestId, Guid associationId, EContactDataKind dataKind, DateTimeOffset accessedAt )
{
    public Guid Id { get; } = id;
    public Guid TransportRequestId { get; } = transportRequestId;
    public Guid AssociationId { get; } = associationId;
    public EContactDataKind DataKind { get; } = dataKind;
    public DateTimeOffset AccessedAt { get; } = accessedAt;
}
