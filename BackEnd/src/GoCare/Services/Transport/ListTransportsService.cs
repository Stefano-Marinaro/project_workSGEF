using GoCare.Data;
using GoCare.Dtos.Transport.Responses;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Transport;

public sealed class ListTransportsService(GoCareDbContext db)
{
    public async Task<List<TransportListItemResponse>> ListMineAsync(Guid caregiver, Guid? assistedId, CancellationToken ct)
    {
        var assistedIds = await db.CaregiverAssistedLinks
            .Where(l => l.CaregiverId == caregiver && l.DeletedAt == null)
            .Select(l => l.AssistedPersonId)
            .ToListAsync(ct);

        var query = db.TransportRequests.Where(t => assistedIds.Contains(t.BeneficiaryId));

        if (assistedId is not null)
            query = query.Where(t => t.BeneficiaryId == assistedId);

        return await query
            .OrderByDescending(t => t.DepartureDateHour)
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
