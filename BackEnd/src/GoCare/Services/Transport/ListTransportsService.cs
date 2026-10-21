using GoCare.Data;
using GoCare.Dtos.Transport.Responses;
using GoCare.Models.Enums;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Transport;

public sealed class ListTransportsService(GoCareDbContext db)
{
    public async Task<List<TransportListItemResponse>> ListMineAsync(
        Guid caregiver, Guid? assistedId, ETripTimeframe? timeframe, CancellationToken ct)
    {
        var assistedIds = await db.CaregiverAssistedLinks
            .Where(l => l.CaregiverId == caregiver && l.DeletedAt == null)
            .Select(l => l.AssistedPersonId)
            .ToListAsync(ct);

        var query = db.TransportRequests.Where(t => assistedIds.Contains(t.BeneficiaryId));

        if (assistedId is not null)
            query = query.Where(t => t.BeneficiaryId == assistedId);

        if (timeframe == ETripTimeframe.Upcoming)
        {
            query = query.Where(t => t.RequestStatus == ETripRequestStatus.Pending
                || t.RequestStatus == ETripRequestStatus.Confirmed
                || t.RequestStatus == ETripRequestStatus.InProgress);
        }
        else if (timeframe == ETripTimeframe.Past)
        {
            query = query.Where(t => t.RequestStatus == ETripRequestStatus.Completed
                || t.RequestStatus == ETripRequestStatus.Cancelled
                || t.RequestStatus == ETripRequestStatus.NotCovered);
        }

        var orderedQuery = timeframe == ETripTimeframe.Upcoming
            ? query.OrderBy(t => t.DepartureDateHour)
            : query.OrderByDescending(t => t.DepartureDateHour);

        return await orderedQuery
            .Select(t => new TransportListItemResponse(
                t.Id, t.TripType, t.TripDirection, t.DepartureDateHour, t.ReturnDateHour,
                t.StartAddress, t.EndAddress, t.RequestStatus,
                t.AssignedAssociationId,
                t.AssignedAssociationId == null
                    ? null
                    : db.Associations.Where(a => a.Id == t.AssignedAssociationId).Select(a => a.Name).FirstOrDefault()))
            .ToListAsync(ct);
    }
}
