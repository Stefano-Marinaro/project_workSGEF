using GoCare.Application.Models.Enums;

namespace GoCare.Application.Models.Domain;

public sealed class TripStatusTransition(
    Guid id,
    Guid transportRequestId,
    ETripTransitionStatus status,
    Guid madeByAssociationId,
    string OperatorLabel,
    DateTimeOffset timestamp
    )
{
    public Guid Id { get; } = id;
    public Guid TransportRequestId { get; } = transportRequestId;
    public ETripTransitionStatus Status { get; } = status;
    public Guid MadeByAssociationId { get; } = madeByAssociationId;
    public string OperatorLabel { get; } = OperatorLabel; // chi ha cambiato lo stato, stringa semplice
    public DateTimeOffset Timestamp { get; } = timestamp;

}
    
