using GoCare.Application.Models.Enums;

namespace GoCare.Application.Models.Domain;

public sealed class TransportRequestRejection
{
    private TransportRequestRejection() { } // costruttore vuoto: EF (materializzazione dal DB)

    public TransportRequestRejection(
        Guid id,
        Guid transportRequestId,
        Guid associationId,
        ERejectionKind kind,
        string? reason,
        DateTimeOffset rejectedAt)
    {
        Id = id;
        TransportRequestId = transportRequestId;
        AssociationId = associationId;
        Kind = kind;
        Reason = reason;
        RejectedAt = rejectedAt;
    }

    public Guid Id { get; }
    public Guid TransportRequestId { get; }
    public Guid AssociationId { get; }
    public ERejectionKind Kind { get; }
    public string? Reason { get; }
    public DateTimeOffset RejectedAt { get; }
}
