using GoCare.Application.Models.Enums;

namespace GoCare.Application.Models.Domain;

public sealed class TripStatusTransition(
    Guid id,
    Guid transportRequestId,
    ETripTransitionStatus status,
    Guid madeByAssociationId,
    string operatorLabel,
    DateTimeOffset occurredAt
    )
{
    public Guid Id { get; } = id;
    public Guid TransportRequestId { get; } = transportRequestId;
    public ETripTransitionStatus Status { get; } = status;
    public Guid MadeByAssociationId { get; } = madeByAssociationId;
    public string OperatorLabel { get; } = operatorLabel; // chi ha cambiato lo stato , stringa semplice Es: "Mauro Rossi, autista"
    public DateTimeOffset OccurredAt { get; } = occurredAt;

}
    
