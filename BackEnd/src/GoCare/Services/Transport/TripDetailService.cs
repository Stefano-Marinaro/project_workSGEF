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

        // I contatti dell'associazione si vedono solo a pagamento confermato (UC 5.2, CG-31):
        // "solo il webhook, non il ritorno dal browser" segna un pagamento come riuscito, quindi
        // basta controllare lo stato già salvato, nessuna chiamata a Stripe da qui.
        var isPaid = await db.Payments.AnyAsync(
            p => p.TransportRequestId == tripId && p.Status == EPaymentStatus.Succeeded, ct);

        string? associationName = null;
        List<string>? associationPhones = null;

        if (trip.AssignedAssociationId is not null && isPaid)
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
            isPaid,
            associationName,
            associationPhones);
    }
}
