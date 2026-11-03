using GoCare.Data;
using GoCare.Dtos.Transport.Responses;
using GoCare.Errors;
using GoCare.Models.Enums;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Transport;

// Dettaglio di una singola richiesta pendente (UC 6.2): stessi dati minimi della lista (PA-02).
public sealed class PendingRequestDetailService(GoCareDbContext db)
{
    public async Task<AssociationPendingRequestResponse> GetAsync(Guid associationId, Guid transportRequestId, CancellationToken ct)
    {
        return await (
            from t in db.TransportRequests
            join candidate in db.TransportRequestCandidates.Where(c => c.AssociationId == associationId)
                on t.Id equals candidate.TransportRequestId
            join beneficiary in db.AssistedPeople on t.BeneficiaryId equals beneficiary.Id
            where t.Id == transportRequestId && t.RequestStatus == ETripRequestStatus.Pending
            select new AssociationPendingRequestResponse(
                t.Id,
                beneficiary.Name + " " + beneficiary.Surname,
                t.StartAddress.Street,
                t.DepartureDateHour,
                t.TripDirection,
                candidate.DistanceToReachKm,
                t.DistanceKm))
            .SingleOrDefaultAsync(ct)
            ?? throw new NotFoundException("Richiesta non trovata o non più disponibile per la tua associazione.");
    }
}
