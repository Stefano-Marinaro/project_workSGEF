using GoCare.Data;
using GoCare.Errors;
using GoCare.Models.Domain;
using GoCare.Models.Enums;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Transport;

// disdetta di un trasporto già accettato: la richiesta torna alle altre candidate, oppure diventa NotCovered
public sealed class CancelAcceptedTransportService(
    GoCareDbContext db, CoverageEvaluator coverage, NotificationDispatcher notifications)
{
    public async Task CancelAsync(Guid transportId, Guid associationId, string reason, CancellationToken ct)
    {
        var transport = await db.TransportRequests
            .SingleOrDefaultAsync(t => t.Id == transportId && t.AssignedAssociationId == associationId, ct)
            ?? throw new NotFoundException("Trasporto non trovato o non assegnato alla tua associazione.");

        if (transport.RequestStatus is not ETripRequestStatus.Confirmed)
            throw new ConflictException("Si può disdire solo un trasporto confermato e non ancora iniziato.");

        var now = DateTimeOffset.UtcNow;

        // la disdetta conta come rifiuto: la richiesta non tornerà più nella lista di questa associazione
        db.TransportRequestRejections.Add(new TransportRequestRejection(
            Guid.NewGuid(), transportId, associationId, ERejectionKind.CancelledAfterAcceptance, reason.Trim(), now));

        transport.ReleaseByAssociation();

        // i link dell'operatore di questa associazione non devono più funzionare
        var operatorLinks = await db.TripOperatorLinks
            .Where(l => l.TransportRequestId == transportId && l.RevokedAt == null)
            .ToListAsync(ct);
        foreach (var link in operatorLinks)
            link.Revoke(now);

        var notCovered = await coverage.IsNotCoveredAsync(transport, ct);
        if (notCovered)
            transport.RequestNotCovered(now);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            // il trasporto è cambiato nel frattempo (es. l'operatore l'ha appena avviato)
            throw new ConflictException("Il trasporto è stato modificato nel frattempo: riprova.");
        }

        await notifications.DispatchAsync(
            ENotificationSubject.Person,
            transport.RequestedById,
            ENotificationType.TripCancelledByAssociation,
            "Trasporto disdetto dall'associazione",
            notCovered
                ? $"L'associazione ha disdetto il trasporto ({reason.Trim()}) e nessun'altra associazione è disponibile."
                : $"L'associazione ha disdetto il trasporto ({reason.Trim()}). La richiesta è di nuovo visibile alle altre associazioni della zona.",
            ENotificationChannel.Push | ENotificationChannel.Email,
            transport.Id,
            ct);
    }
}
