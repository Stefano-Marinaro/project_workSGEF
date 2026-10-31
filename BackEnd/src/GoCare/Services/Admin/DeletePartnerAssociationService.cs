using GoCare.Data;
using GoCare.Errors;
using GoCare.Models.Domain;
using GoCare.Models.Enums;
using GoCare.Services.Transport;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Admin;

// L'admin elimina un'associazione partner (UC 11.4 · AD-04). Soft delete: account e profilo restano per lo storico.
public sealed class DeletePartnerAssociationService(
    GoCareDbContext db, CoverageEvaluator coverage, NotificationDispatcher notifications)
{
    private const string RejectionReason = "Associazione eliminata dall'amministratore";

    public async Task DeleteAsync(Guid associationId, CancellationToken ct)
    {
        var association = await db.Associations.SingleOrDefaultAsync(a => a.Id == associationId, ct)
            ?? throw new NotFoundException("Associazione non trovata.");

        if (association.DeletedAt is not null)
            throw new ConflictException("L'associazione è già stata eliminata.");

        // non si lascia a piedi nessuno: prima vanno conclusi o disdetti i viaggi in carico
        var hasActiveTransports = await db.TransportRequests.AnyAsync(t =>
            t.AssignedAssociationId == associationId
            && (t.RequestStatus == ETripRequestStatus.Confirmed || t.RequestStatus == ETripRequestStatus.InProgress), ct);
        if (hasActiveTransports)
            throw new ConflictException("L'associazione ha trasporti confermati o in corso: non può essere eliminata.");

        var account = await db.Accounts.SingleAsync(a => a.Id == associationId, ct);
        var now = DateTimeOffset.UtcNow;

        account.MarkDeleted(now);    // blocca il login (CanLogIn richiede Active)
        association.MarkDeleted(now); // esclusa dalle candidate delle nuove richieste (CreateTransportService)

        var sessions = await db.RefreshTokens
            .Where(t => t.AccountId == associationId && t.RevokedAt == null)
            .ToListAsync(ct);
        foreach (var session in sessions)
            session.Revoke(now);

        var operatorLinks = await db.TripOperatorLinks
            .Where(l => l.RevokedAt == null
                && db.TransportRequests.Any(t => t.Id == l.TransportRequestId && t.AssignedAssociationId == associationId))
            .ToListAsync(ct);
        foreach (var link in operatorLinks)
            link.Revoke(now);

        // le richieste in attesa di cui era candidata (e su cui non aveva ancora deciso) contano come rifiutate:
        // se era l'ultima candidata rimasta, la richiesta diventa NotCovered
        var pending = await db.TransportRequests
            .Where(t => t.RequestStatus == ETripRequestStatus.Pending
                && db.TransportRequestCandidates.Any(c => c.TransportRequestId == t.Id && c.AssociationId == associationId)
                && !db.TransportRequestRejections.Any(r => r.TransportRequestId == t.Id && r.AssociationId == associationId))
            .ToListAsync(ct);

        foreach (var transport in pending)
            db.TransportRequestRejections.Add(new TransportRequestRejection(
                Guid.NewGuid(), transport.Id, associationId, ERejectionKind.Declined, RejectionReason, now));

        var notCovered = new List<TransportRequest>();
        foreach (var transport in pending)
        {
            if (!await coverage.IsNotCoveredAsync(transport, ct))
                continue;

            transport.RequestNotCovered(now);
            notCovered.Add(transport);
        }

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            // una richiesta è cambiata nel frattempo (es. appena accettata da lei)
            throw new ConflictException("Una richiesta dell'associazione è cambiata nel frattempo: riprova.");
        }

        foreach (var transport in notCovered)
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
