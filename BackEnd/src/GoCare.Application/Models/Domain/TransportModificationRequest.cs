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

    public void Approve(DateTimeOffset at)
    {
        if (Status != EModificationRequestStatus.PendingApproval)
            throw new InvalidOperationException("Impossibile approvare una richiesta non in stato 'PendingApproval'.");

        Status = EModificationRequestStatus.Approved;
        ResolvedAt = at;
    }

    public void Reject(DateTimeOffset at, string outcomeMessage)
    {
        if (Status != EModificationRequestStatus.PendingApproval)
            throw new InvalidOperationException("Impossibile rifiutare una richiesta non in stato 'PendingApproval'.");

        if (string.IsNullOrWhiteSpace(outcomeMessage))
            throw new InvalidOperationException("E' necessario inserire una motivazione per il rifiuto");

        Status = EModificationRequestStatus.Rejected;
        ResolvedAt = at;
        OutcomeMessage = outcomeMessage; //qua va capito come gestire i tipi di rifiuto e definirli. Facciamo scrivere il motivo all associazione? facciamo una lista tra cui scegliere?
    }

    public void Retract(DateTimeOffset at)
    {
        if (Status != EModificationRequestStatus.PendingApproval)
            throw new InvalidOperationException("Impossibile ritirare una richiesta non in stato 'PendingApproval'.");

        Status = EModificationRequestStatus.Retracted;
        ResolvedAt = at;
    }
}

