using GoCare.Data;
using GoCare.Dtos.Transport.Responses;
using GoCare.Errors;
using GoCare.Models.Domain;
using GoCare.Models.Enums;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Transport;

// recapiti completi di richiedente e assistito: solo per l'associazione assegnata e solo finché il viaggio è attivo.
// Ogni lettura viene registrata in ContactAccessLog (PA-04).
public sealed class RequesterContactsService(GoCareDbContext db)
{
    public async Task<RequesterContactsResponse> GetAsync(Guid associationId, Guid transportId, CancellationToken ct)
    {
        var transport = await db.TransportRequests.AsNoTracking()
            .SingleOrDefaultAsync(t => t.Id == transportId && t.AssignedAssociationId == associationId, ct)
            ?? throw new NotFoundException("Trasporto non trovato o non assegnato alla tua associazione.");

        if (transport.RequestStatus is not (ETripRequestStatus.Confirmed or ETripRequestStatus.InProgress))
            throw new ConflictException("I recapiti sono disponibili solo per un trasporto confermato o in corso.");

        var requester = await db.Persons.AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == transport.RequestedById, ct)
            ?? throw new NotFoundException("Richiedente non trovato.");

        var beneficiary = await db.AssistedPeople.AsNoTracking()
            .SingleOrDefaultAsync(a => a.Id == transport.BeneficiaryId, ct)
            ?? throw new NotFoundException("Assistito non trovato.");

        var now = DateTimeOffset.UtcNow;
        db.ContactAccessLogs.AddRange(
            new ContactAccessLog(Guid.NewGuid(), transportId, associationId, EContactDataKind.Requester, now),
            new ContactAccessLog(Guid.NewGuid(), transportId, associationId, EContactDataKind.Beneficiary, now));
        await db.SaveChangesAsync(ct);

        return new RequesterContactsResponse(
            requester.Name,
            requester.Surname,
            requester.Phone,
            requester.Email,
            transport.ReferencePhone,
            transport.ReferenceEmail,
            beneficiary.Name,
            beneficiary.Surname,
            beneficiary.Phone);
    }
}
