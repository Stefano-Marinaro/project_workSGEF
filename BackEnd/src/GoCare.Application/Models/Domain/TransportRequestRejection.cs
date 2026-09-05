using GoCare.Application.Models.Enums;

namespace GoCare.Application.Models.Domain;

public sealed class TransportRequestRejection(
    Guid id,
    Guid transportRequestId,
    Guid associationId,
    ERejectionKind kind,
    string? reason,
    DateTimeOffset rejectedAt)
{
    public Guid Id { get; } = id;
    public Guid TransportRequestId { get; } = transportRequestId;
    public Guid AssociationId { get; } = associationId;
    public ERejectionKind Kind { get; } = kind;
    public string? Reason { get; } = reason;
    public DateTimeOffset RejectedAt { get; } = rejectedAt;
}
