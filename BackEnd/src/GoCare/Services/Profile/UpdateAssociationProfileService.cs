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
        List<string> coveredProvinces,
        string? availabilityHours,
        CancellationToken ct)
    {
        var association = await db.Associations.SingleOrDefaultAsync(a => a.Id == accountId && a.DeletedAt == null, ct)
            ?? throw new NotFoundException("Profilo associazione non trovato.");

        association.UpdateProfile(name, headquarter, phones, coveredProvinces, availabilityHours);

        await db.SaveChangesAsync(ct);
    }
}
