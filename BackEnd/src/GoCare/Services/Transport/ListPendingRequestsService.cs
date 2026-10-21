using GoCare.Data;
using GoCare.Dtos.Transport.Responses;
using GoCare.Models.Enums;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Transport;

// richieste in attesa che l'associazione può ancora accettare
public sealed class ListPendingRequestsService(GoCareDbContext db)
{
    public async Task<List<AssociationTransportResponse>> ListAsync(Guid associationId, CancellationToken ct)
    {
        return await db.TransportRequests.AsNoTracking()
            .Where(t => t.RequestStatus == ETripRequestStatus.Pending
                // sono fra le candidate di QUESTA richiesta
                && db.TransportRequestCandidates.Any(c => c.TransportRequestId == t.Id && c.AssociationId == associationId)
                // e non l'ho già rifiutata o disdetta
                && !db.TransportRequestRejections.Any(r => r.TransportRequestId == t.Id && r.AssociationId == associationId))
            .OrderBy(t => t.DepartureDateHour) // prima le più urgenti
            .Select(t => new AssociationTransportResponse(
                t.Id, t.TripType, t.TripDirection, t.DepartureDateHour, t.ReturnDateHour,
                t.StartAddress, t.EndAddress, t.RequestStatus))
            .ToListAsync(ct);
    }
}