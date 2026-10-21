using GoCare.Data;
using GoCare.Errors;
using GoCare.Models.Domain;
using GoCare.Models.Enums;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Transport;

public sealed class DeclineTransportRequestService(
    GoCareDbContext db, CoverageEvaluator coverage, NotificationDispatcher notifications)
{
    private const int ReasonMaxLength = 300; // come la colonna Reason di TransportRequestRejection

    public async Task DeclineAsync(Guid transportId, Guid associationId, string? reason, CancellationToken ct)
    {
        reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        if (reason is { Length: > ReasonMaxLength })
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["reason"] = [$"Il motivo non può superare {ReasonMaxLength} caratteri."]
            });

        var transport = await db.TransportRequests.SingleOrDefaultAsync(t => t.Id == transportId, ct)
            ?? throw new NotFoundException("Richiesta di trasporto non trovata.");

        var isCandidate = await db.TransportRequestCandidates.AnyAsync(
            c => c.TransportRequestId == transportId && c.AssociationId == associationId, ct);
        if (!isCandidate)
            throw new ForbiddenException("La tua associazione non è candidata per questa richiesta.");

        if (transport.RequestStatus is not ETripRequestStatus.Pending)
            throw new ConflictException("La richiesta non è più in attesa: non può essere rifiutata.");

        var alreadyDeclined = await db.TransportRequestRejections.AnyAsync(
            r => r.TransportRequestId == transportId && r.AssociationId == associationId, ct);
        if (alreadyDeclined)
            throw new ConflictException("La tua associazione ha già rifiutato questa richiesta.");

        var now = DateTimeOffset.UtcNow;
        db.TransportRequestRejections.Add(new TransportRequestRejection(
            Guid.NewGuid(), transportId, associationId, ERejectionKind.Declined, reason, now));

        var notCovered = await coverage.IsNotCoveredAsync(transport, ct);
        if (notCovered)
            transport.RequestNotCovered(now);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            // un'altra associazione ha accettato nello stesso istante (xmin)
            throw new ConflictException("La richiesta è stata modificata nel frattempo: non è più in attesa.");
        }

        if (notCovered)
            await notifications.DispatchAsync(
                ENotificationSubject.Person,
                transport.RequestedById,
                ENotificationType.RequestNotCovered,
                "Richiesta di trasporto non coperta",
                "Nessuna associazione disponibile ha potuto prendere in carico la tua richiesta di trasporto.",
                ENotificationChannel.Push | ENotificationChannel.Email,
                transport.Id,
                ct);
    }
}
