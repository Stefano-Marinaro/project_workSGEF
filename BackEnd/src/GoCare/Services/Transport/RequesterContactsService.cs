using GoCare.Data;
using GoCare.Dtos.Transport.Responses;
using GoCare.Errors;
using GoCare.Models.Domain;
using GoCare.Models.Enums;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Transport;

// Recapiti completi del richiedente, visibili all'associazione assegnataria solo dopo
// l'avvenuto pagamento (UC 6.5, PA-02) — speculare a TripDetailService lato caregiver.
public sealed class RequesterContactsService(GoCareDbContext db)
{
    public async Task<RequesterContactsResponse> GetAsync(Guid associationId, Guid transportRequestId, CancellationToken ct)
    {
        var trip = await db.TransportRequests.SingleOrDefaultAsync(
            t => t.Id == transportRequestId && t.AssignedAssociationId == associationId, ct)
            ?? throw new NotFoundException("Trasporto non trovato o non assegnato alla tua associazione.");

        var isPaid = await db.Payments.AnyAsync(
            p => p.TransportRequestId == transportRequestId && p.Status == EPaymentStatus.Succeeded, ct);

        if (!isPaid)
            throw new ForbiddenException("Il trasporto non è stato ancora pagato.");

        db.ContactAccessLogs.Add(new ContactAccessLog(
            Guid.NewGuid(), transportRequestId, associationId, EContactDataKind.Requester, DateTimeOffset.UtcNow));
        await db.SaveChangesAsync(ct);

        return new RequesterContactsResponse(trip.ReferencePhone, trip.ReferenceEmail);
    }
}
