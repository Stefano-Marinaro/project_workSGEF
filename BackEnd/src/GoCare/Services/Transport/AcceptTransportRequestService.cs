using GoCare.Data;
using GoCare.Errors;
using GoCare.Models.Enums;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Transport;

public sealed class AcceptTransportRequestService(GoCareDbContext db, NotificationDispatcher notifications)
{
    public async Task AcceptAsync(Guid transportId, Guid associationId, CancellationToken ct)
    {
        var transport = await db.TransportRequests.SingleOrDefaultAsync(t => t.Id == transportId, ct)
            ?? throw new NotFoundException("Richiesta di trasporto non trovata.");

        var isCandidate = await db.TransportRequestCandidates.AnyAsync(
            c => c.TransportRequestId == transportId && c.AssociationId == associationId, ct);
        if (!isCandidate)
            throw new ForbiddenException("La tua associazione non è candidata per questa richiesta.");

        if (transport.RequestStatus is not ETripRequestStatus.Pending)
            throw new ConflictException("La richiesta non è più in attesa: è già stata accettata o non è più disponibile.");

        var now = DateTimeOffset.UtcNow;
        transport.Accept(associationId, now);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            // un'altra associazione ha accettato nello stesso istante: "prima accettazione vince" (xmin).
            throw new ConflictException("Un'altra associazione ha già accettato questa richiesta nel frattempo.");
        }

        await notifications.DispatchAsync(
            ENotificationSubject.Person,
            transport.RequestedById,
            ENotificationType.RequestAccepted,
            "Richiesta di trasporto accettata",
            "La tua richiesta di trasporto è stata presa in carico da un'associazione.",
            ENotificationChannel.Push | ENotificationChannel.Email,
            transport.Id,
            ct);
    }
}
