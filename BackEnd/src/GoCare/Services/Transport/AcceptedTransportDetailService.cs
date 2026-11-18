using GoCare.Data;
using GoCare.Dtos.Transport.Responses;
using GoCare.Errors;
using GoCare.Models.Enums;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Transport;

public sealed class AcceptedTransportDetailService(GoCareDbContext db)
{
    public async Task<AssociationTransportResponse> GetAsync(Guid associationId, Guid tripId, CancellationToken ct)
    {
        var trip = await db.TransportRequests.AsNoTracking()
            .SingleOrDefaultAsync(t => t.Id == tripId && t.AssignedAssociationId == associationId, ct)
            ?? throw new NotFoundException("Trasporto non trovato o non assegnato alla tua associazione.");

        // Serve al frontend per bloccare l'assegnazione del volontario prima del pagamento
        var isPaid = await db.Payments.AnyAsync(
            p => p.TransportRequestId == tripId && p.Status == EPaymentStatus.Succeeded, ct);

        return new AssociationTransportResponse(
            trip.Id, trip.TripType, trip.TripDirection, trip.DepartureDateHour, trip.ReturnDateHour,
            trip.StartAddress, trip.EndAddress, trip.RequestStatus, isPaid);
    }
}
