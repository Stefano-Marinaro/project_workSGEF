using GoCare.Data;
using GoCare.Dtos.Transport.Responses;
using GoCare.Errors;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Transport;

public sealed class TimelineService(GoCareDbContext db)
{
    public async Task<List<TimelineEntryResponse>> GetTimelineAsync(Guid tripId, Guid caregiver, CancellationToken ct)
    {
        var trip = await db.TransportRequests.SingleOrDefaultAsync(t => t.Id == tripId, ct)
            ?? throw new NotFoundException("Trasporto non trovato.");

        var isLinked = await db.CaregiverAssistedLinks.AnyAsync(
            l => l.CaregiverId == caregiver && l.AssistedPersonId == trip.BeneficiaryId && l.DeletedAt == null, ct);
        if (!isLinked)
            throw new ForbiddenException("Non sei collegato a questo assistito.");

        return await db.TripStatusTransitions
            .Where(t => t.TransportRequestId == tripId)
            .OrderBy(t => t.OccurredAt)
            .Select(t => new TimelineEntryResponse(t.Status, t.OperatorLabel, t.OccurredAt))
            .ToListAsync(ct);
    }
}
