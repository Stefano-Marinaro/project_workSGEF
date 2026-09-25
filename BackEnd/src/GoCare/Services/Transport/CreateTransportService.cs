using GoCare.Data;
using GoCare.Models.Enums;
using GoCare.Models.Domain;

using Microsoft.EntityFrameworkCore;
using GoCare.Errors;

namespace GoCare.Services.Transport;

public sealed class CreateTransportService(GoCareDbContext db)
{
    public async Task<Guid> CreateTransportAsync(
        Guid requestedById,
        Guid beneficiaryId,
        ETripType tripType,
        ETripDirection tripDirection,
        DateTimeOffset departureDateHour,
        DateTimeOffset? returnDateHour,
        Address startAddress,
        Address endAddress,
        Address? returnEndAddress,
        string referencePhone,
        string referenceEmail,
        IReadOnlyList<(string Name, string Surname, string Relationship, string Phone)> companion,
        CancellationToken ct)
    {
        var isLinked = await db.CaregiverAssistedLinks.AnyAsync(
            l => l.CaregiverId == requestedById && l.AssistedPersonId == beneficiaryId && l.DeletedAt == null, ct);
        if (!isLinked)
            throw new ForbiddenException("Non sei collegato a questo assistito.");

        var now = DateTimeOffset.UtcNow;    
        var transportId = Guid.NewGuid();

        var transport = new TransportRequest(
            transportId, requestedById, beneficiaryId, tripType, tripDirection, departureDateHour,
            returnDateHour, startAddress, endAddress, returnEndAddress, referencePhone, referenceEmail, now);

        db.TransportRequests.Add(transport);

        foreach (var c in companion)
            db.Companions.Add(new Companion(Guid.NewGuid(), transportId, c.Name, c.Surname, c.Relationship, c.Phone));

        var candidateAssociationIds = await db.Associations
         .Where(a => a.Status == EAccreditationStatus.Accredited
                  && a.DeletedAt == null
                  && a.CoveredProvinces != null
                  && a.CoveredProvinces.Contains(startAddress.Province))
         .Select(a => a.Id)
         .ToListAsync(ct);

        foreach (var associationId in candidateAssociationIds)
            db.TransportRequestCandidates.Add(new TransportRequestCandidate(transportId, associationId));

        await db.SaveChangesAsync(ct);

        return transportId;
    }
}


