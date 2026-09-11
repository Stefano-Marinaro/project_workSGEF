using GoCare.Application.Models.Enums;

namespace GoCare.Application.Models.Domain;

public sealed class TripStatusTransition
{
    private TripStatusTransition() { } // costruttore vuoto: EF (materializzazione dal DB)

    public TripStatusTransition(
        Guid id,
        Guid transportRequestId,
        ETripTransitionStatus status,
        Guid madeByAssociationId,
        string operatorLabel,
        DateTimeOffset occurredAt)
    {
        Id = id;
        TransportRequestId = transportRequestId;
        Status = status;
        MadeByAssociationId = madeByAssociationId;
        OperatorLabel = operatorLabel;
        OccurredAt = occurredAt;
    }

    public Guid Id { get; }
    public Guid TransportRequestId { get; }
    public ETripTransitionStatus Status { get; }
    public Guid MadeByAssociationId { get; }
    public string OperatorLabel { get; } = null!; // chi ha cambiato lo stato , stringa semplice Es: "Mauro Rossi, autista"
    public DateTimeOffset OccurredAt { get; }
}
