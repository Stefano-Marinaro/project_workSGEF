using GoCare.Data;
using GoCare.Dtos.Transport.Responses;
using GoCare.Errors;
using GoCare.Models.Domain;
using GoCare.Models.Enums;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Transport;

public sealed class AdvanceTripStatusService(GoCareDbContext db)
{
    private static readonly ETripTransitionStatus[] OneWaySequence =
    [
        ETripTransitionStatus.InCharge,
        ETripTransitionStatus.Arriving,
        ETripTransitionStatus.Completed,
    ];

    private static readonly ETripTransitionStatus[] RoundTripSequence =
    [
        ETripTransitionStatus.InCharge,
        ETripTransitionStatus.Arriving,
        ETripTransitionStatus.OnSite,
        ETripTransitionStatus.Returning,
        ETripTransitionStatus.Completed,
    ];

    public async Task<OperatorTripStatusResponse> AdvanceAsync(
        string token, ETripOperatorAction action, string operatorLabel, CancellationToken ct)
    {
        var trip = await ResolveTripAsync(token, ct);
        var transitions = await LoadTransitionsAsync(trip.Id, ct);
        var (effectiveStatus, isSuspended) = Resolve(transitions);
        var sequence = trip.TripDirection == ETripDirection.RoundTrip ? RoundTripSequence : OneWaySequence;

        ETripTransitionStatus? statusToLog = action switch
        {
            ETripOperatorAction.Resume when isSuspended && effectiveStatus is not null
                => effectiveStatus,
            ETripOperatorAction.Suspend when !isSuspended && effectiveStatus is not null
                && effectiveStatus != ETripTransitionStatus.Completed
                => ETripTransitionStatus.Suspended,
            ETripOperatorAction.Advance when !isSuspended
                => NextInSequence(sequence, effectiveStatus),
            _ => null,
        };

        if (statusToLog is null)
            throw new ConflictException("Azione non valida per lo stato corrente del viaggio.");

        db.TripStatusTransitions.Add(new TripStatusTransition(
            Guid.NewGuid(), trip.Id, statusToLog.Value, trip.AssignedAssociationId!.Value, operatorLabel, DateTimeOffset.UtcNow));

        if (statusToLog == ETripTransitionStatus.InCharge)
            trip.StartExecution();
        else if (statusToLog == ETripTransitionStatus.Completed)
            trip.CompleteExecution();

        await db.SaveChangesAsync(ct);

        var newIsSuspended = statusToLog == ETripTransitionStatus.Suspended;
        var newEffectiveStatus = newIsSuspended ? effectiveStatus : statusToLog;

        return ToResponse(trip.TripDirection, newEffectiveStatus, newIsSuspended);
    }

    private static ETripTransitionStatus? NextInSequence(ETripTransitionStatus[] sequence, ETripTransitionStatus? current)
    {
        if (current is null)
            return sequence[0];

        var index = Array.IndexOf(sequence, current.Value);
        if (index < 0 || index == sequence.Length - 1)
            return null;

        return sequence[index + 1];
    }

    private static (ETripTransitionStatus? EffectiveStatus, bool IsSuspended) Resolve(
        IReadOnlyList<TripStatusTransition> transitions)
    {
        if (transitions.Count == 0)
            return (null, false);

        var last = transitions[^1];
        if (last.Status != ETripTransitionStatus.Suspended)
            return (last.Status, false);

        var lastActive = transitions.LastOrDefault(t => t.Status != ETripTransitionStatus.Suspended);
        return (lastActive?.Status, true);
    }

    private static OperatorTripStatusResponse ToResponse(
        ETripDirection direction, ETripTransitionStatus? effectiveStatus, bool isSuspended)
    {
        var sequence = direction == ETripDirection.RoundTrip ? RoundTripSequence : OneWaySequence;
        var isCompleted = effectiveStatus == ETripTransitionStatus.Completed;

        return new OperatorTripStatusResponse(
            CurrentStatus: isSuspended ? ETripTransitionStatus.Suspended : effectiveStatus,
            NextStatus: isSuspended || isCompleted ? null : NextInSequence(sequence, effectiveStatus),
            IsSuspended: isSuspended,
            CanSuspend: !isSuspended && effectiveStatus is not null && !isCompleted,
            CanResume: isSuspended,
            IsCompleted: isCompleted);
    }

    private async Task<TransportRequest> ResolveTripAsync(string token, CancellationToken ct)
    {
        var link = await db.TripOperatorLinks.SingleOrDefaultAsync(l => l.Token == token, ct)
            ?? throw new NotFoundException("Link non valido.");

        if (!link.IsUsable(DateTimeOffset.UtcNow))
            throw new ForbiddenException("Il link è scaduto o è stato revocato.");

        return await db.TransportRequests.SingleOrDefaultAsync(t => t.Id == link.TransportRequestId, ct)
            ?? throw new NotFoundException("Trasporto non trovato.");
    }

    private async Task<List<TripStatusTransition>> LoadTransitionsAsync(Guid tripId, CancellationToken ct) =>
        await db.TripStatusTransitions
            .Where(t => t.TransportRequestId == tripId)
            .OrderBy(t => t.OccurredAt)
            .ToListAsync(ct);
}
