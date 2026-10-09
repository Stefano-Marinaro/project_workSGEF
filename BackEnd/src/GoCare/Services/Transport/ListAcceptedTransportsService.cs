using GoCare.Data;
using GoCare.Dtos.Transport.Responses;
using GoCare.Models.Enums;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Transport;

public sealed class ListAcceptedTransportsService(GoCareDbContext db)
{
    public async Task<List<AssociationTransportResponse>> ListAsync(Guid associationId, CancellationToken ct)
    {
        return await db.TransportRequests
            .Where(t => t.AssignedAssociationId == associationId
                && (t.RequestStatus == ETripRequestStatus.Confirmed || t.RequestStatus == ETripRequestStatus.InProgress))
            .OrderBy(t => t.DepartureDateHour)
            .Select(t => new AssociationTransportResponse(
                t.Id, t.TripType, t.TripDirection, t.DepartureDateHour, t.ReturnDateHour,
                t.StartAddress, t.EndAddress, t.RequestStatus))
            .ToListAsync(ct);
    }
}
