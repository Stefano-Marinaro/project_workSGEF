using GoCare.Data;
using GoCare.Dtos.Transport.Responses;
using GoCare.Models.Enums;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Transport;

// richieste in attesa che l'associazione può ancora accettare
public sealed class ListPendingRequestsService(GoCareDbContext db)
{
    public async Task<List<AssociationPendingRequestResponse>> ListAsync(Guid associationId, CancellationToken ct)
    {
        // Dati minimi (PA-02): nome del beneficiario, via di partenza, data, direzione, km per
        // raggiungerlo e km del tragitto — niente indirizzo completo né contatti prima del pagamento.
        return await (
            from t in db.TransportRequests
            join candidate in db.TransportRequestCandidates.Where(c => c.AssociationId == associationId)
                on t.Id equals candidate.TransportRequestId
            join beneficiary in db.AssistedPeople on t.BeneficiaryId equals beneficiary.Id
            where t.RequestStatus == ETripRequestStatus.Pending
                // e non l'ho già rifiutata o disdetta
                && !db.TransportRequestRejections.Any(r => r.TransportRequestId == t.Id && r.AssociationId == associationId)
            orderby t.DepartureDateHour // prima le più urgenti
            select new AssociationPendingRequestResponse(
                t.Id,
                beneficiary.Name + " " + beneficiary.Surname,
                t.StartAddress.Street,
                t.DepartureDateHour,
                t.TripDirection,
                candidate.DistanceToReachKm,
                t.DistanceKm))
            .ToListAsync(ct);
    }
}