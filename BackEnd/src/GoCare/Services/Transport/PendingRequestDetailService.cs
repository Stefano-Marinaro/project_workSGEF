using GoCare.Data;
using GoCare.Dtos.Transport.Responses;
using GoCare.Errors;
using GoCare.Models.Enums;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Transport;

// dettaglio di una richiesta in attesa: stesso filtro di ListPendingRequestsService, su un solo id
public sealed class PendingRequestDetailService(GoCareDbContext db)
{
    public async Task<AssociationTransportResponse> GetAsync(Guid associationId, Guid transportId, CancellationToken ct)
    {
        return await db.TransportRequests.AsNoTracking()
            .Where(t => t.Id == transportId
                && t.RequestStatus == ETripRequestStatus.Pending
                && db.TransportRequestCandidates.Any(c => c.TransportRequestId == t.Id && c.AssociationId == associationId)
                && !db.TransportRequestRejections.Any(r => r.TransportRequestId == t.Id && r.AssociationId == associationId))
            .Select(t => new AssociationTransportResponse(
                t.Id, t.TripType, t.TripDirection, t.DepartureDateHour, t.ReturnDateHour,
                t.StartAddress, t.EndAddress, t.RequestStatus))
            .SingleOrDefaultAsync(ct)
            // 404 anche se esiste ma non è per me: non rivelo richieste altrui
            ?? throw new NotFoundException("Richiesta non trovata o non più disponibile per la tua associazione.");
    }
}
