using GoCare.Data;
using GoCare.Dtos.Transport.Responses;
using GoCare.Models.Enums;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Transport;

public sealed class AssociationHistoryService(GoCareDbContext db)
{
    public async Task<List<AssociationTransportResponse>> GetHistoryAsync(Guid associationId, CancellationToken ct)
    {
        return await db.TransportRequests
            .Where(t => t.AssignedAssociationId == associationId
                && (t.RequestStatus == ETripRequestStatus.Completed || t.RequestStatus == ETripRequestStatus.Cancelled))
            .OrderByDescending(t => t.DepartureDateHour)
            .Select(t => new AssociationTransportResponse(
                t.Id, t.TripType, t.TripDirection, t.DepartureDateHour, t.ReturnDateHour,
                t.StartAddress, t.EndAddress, t.RequestStatus))
            .ToListAsync(ct);
    }
}
