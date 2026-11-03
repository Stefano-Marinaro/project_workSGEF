using GoCare.Data;
using GoCare.Errors;
using GoCare.Models.Domain;
using GoCare.Models.Enums;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GoCare.Services.Transport;

// UC 6.4: rifiuto di una richiesta pendente da parte di una singola associazione candidata.
// Non è uno stato della richiesta (resta "in attesa" per le altre candidate, PA-03 nel documento);
// se però con questo rifiuto TUTTE le candidate hanno risposto no, scatta subito "non coperta"
// (PA-01), stesso CoverageEvaluator usato dal job temporale — niente attesa del prossimo giro.
public sealed class DeclineTransportRequestService(
    GoCareDbContext db, NotificationDispatcher notifications, IOptions<CoverageOptions> options)
{
    public async Task DeclineAsync(Guid transportId, Guid associationId, string? reason, CancellationToken ct)
    {
        var transport = await db.TransportRequests.SingleOrDefaultAsync(t => t.Id == transportId, ct)
            ?? throw new NotFoundException("Richiesta di trasporto non trovata.");

        var isCandidate = await db.TransportRequestCandidates.AnyAsync(
            c => c.TransportRequestId == transportId && c.AssociationId == associationId, ct);
        if (!isCandidate)
            throw new ForbiddenException("La tua associazione non è candidata per questa richiesta.");

        if (transport.RequestStatus is not ETripRequestStatus.Pending)
            throw new ConflictException("La richiesta non è più in attesa: non puoi rifiutarla.");

        var alreadyDeclined = await db.TransportRequestRejections.AnyAsync(
            r => r.TransportRequestId == transportId && r.AssociationId == associationId, ct);
        if (alreadyDeclined)
            throw new ConflictException("Hai già rifiutato questa richiesta.");

        var now = DateTimeOffset.UtcNow;

        var candidateCount = await db.TransportRequestCandidates.CountAsync(c => c.TransportRequestId == transportId, ct);
        var rejectionCountBefore = await db.TransportRequestRejections.CountAsync(r => r.TransportRequestId == transportId, ct);

        db.TransportRequestRejections.Add(new TransportRequestRejection(
            Guid.NewGuid(), transportId, associationId, ERejectionKind.Declined, reason, now));

        var threshold = TimeSpan.FromHours(options.Value.NotCoveredThresholdHours);
        var becomesNotCovered = CoverageEvaluator.IsNotCovered(
            transport.RequestStatus, transport.DepartureDateHour, candidateCount, rejectionCountBefore + 1, now, threshold);

        if (becomesNotCovered)
            transport.RequestNotCovered(now);

        await db.SaveChangesAsync(ct);

        if (becomesNotCovered)
        {
            await notifications.DispatchAsync(
                ENotificationSubject.Person,
                transport.RequestedById,
                ENotificationType.RequestNotCovered,
                "Nessuna associazione disponibile",
                "Non è stato possibile trovare un'associazione per il tuo trasporto in tempo utile. Puoi provare a inviare una nuova richiesta per un altro giorno o orario.",
                ENotificationChannel.Push | ENotificationChannel.Email,
                transport.Id,
                ct);
        }
    }
}
