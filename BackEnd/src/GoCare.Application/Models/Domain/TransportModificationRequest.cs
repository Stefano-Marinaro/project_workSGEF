using GoCare.Application.Models.Enums;

namespace GoCare.Application.Models.Domain;

public sealed class TransportModificationRequest(
    Guid id, 
    Guid transportRequestId,
    EModificationField field,
    string previousValue,
    string proposedValue,
    DateTimeOffset createdAt
    )
{
    public Guid Id { get; } = id;
    public Guid TransportRequestId { get; } = transportRequestId;
    public EModificationField Field { get; } = field;
    public string PreviousValue { get; } = previousValue;
    public string ProposedValue { get; } = proposedValue;
    public EModificationRequestStatus Status { get; private set; } = EModificationRequestStatus.PendingApproval;
    public string? OutcomeMessage { get; private set; }
    public DateTimeOffset CreatedAt { get; } = createdAt;
    public DateTimeOffset? ResolvedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
}
