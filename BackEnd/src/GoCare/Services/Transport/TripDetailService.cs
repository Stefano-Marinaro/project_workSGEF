using GoCare.Data;
using GoCare.Errors;
using GoCare.Dtos.Transport.Responses;

using Microsoft.EntityFrameworkCore;
using GoCare.Models.Enums;

namespace GoCare.Services.Transport;

public class TripDetailService(GoCareDbContext db)
{
    public async Task<TripDetailResponse> TripDetail(Guid tripId, Guid caregiver, CancellationToken ct)
    {
        var trip = await db.TransportRequests.SingleOrDefaultAsync(t => t.Id == tripId, ct);
        if (trip is null)
            throw new NotFoundException("Errore nel recupero dell'ID.");

        var isLinked = await db.CaregiverAssistedLinks.AnyAsync(
            l => l.CaregiverId == caregiver && l.AssistedPersonId == trip.BeneficiaryId && l.DeletedAt == null, ct);
        if (!isLinked)
            throw new ForbiddenException("Non sei collegato a questo assistito.");

        string? associationName = null;
        List<string>? associationPhones = null;

        if (trip.AssignedAssociationId is not null)
        {
            var association = await db.Associations.SingleOrDefaultAsync(a => a.Id == trip.AssignedAssociationId, ct);
            associationName = association?.Name;
            associationPhones = association?.Phones;
        }

        return new TripDetailResponse(
            trip.TripType,
            trip.TripDirection,
            trip.DepartureDateHour,
            trip.ReturnDateHour,
            trip.StartAddress,
            trip.EndAddress,
            trip.ReturnEndAddress,
            trip.RequestStatus,
            associationName,
            associationPhones);
    }
}
