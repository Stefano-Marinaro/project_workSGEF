using GoCare.Data;
using GoCare.Dtos.Transport.Responses;
using GoCare.Errors;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Transport;

public sealed class AcceptedTransportDetailService(GoCareDbContext db)
{
    public async Task<AssociationTransportResponse> GetAsync(Guid associationId, Guid tripId, CancellationToken ct)
    {
        var trip = await db.TransportRequests.AsNoTracking()
            .SingleOrDefaultAsync(t => t.Id == tripId && t.AssignedAssociationId == associationId, ct)
            ?? throw new NotFoundException("Trasporto non trovato o non assegnato alla tua associazione.");

        return new AssociationTransportResponse(
            trip.Id, trip.TripType, trip.TripDirection, trip.DepartureDateHour, trip.ReturnDateHour,
            trip.StartAddress, trip.EndAddress, trip.RequestStatus);
    }
}
