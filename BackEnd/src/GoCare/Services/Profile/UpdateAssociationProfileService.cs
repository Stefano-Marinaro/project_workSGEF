using GoCare.Data;
using GoCare.Errors;
using GoCare.Models.Domain;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Profile;

public sealed class UpdateAssociationProfileService(GoCareDbContext db)
{
    public async Task UpdateAsync(
        Guid accountId,
        string name,
        Address headquarter,
        List<string> phones,
        List<string> coveredComuni,
        string? availabilityHours,
        CancellationToken ct)
    {
        var association = await db.Associations.SingleOrDefaultAsync(a => a.Id == accountId && a.DeletedAt == null, ct)
            ?? throw new NotFoundException("Profilo associazione non trovato.");

        association.UpdateProfile(name, headquarter, phones, availabilityHours);
        await CoveredComuniWriter.ReplaceAsync(db, association.Id, coveredComuni, ct);
        await db.SaveChangesAsync(ct);
    }
}
