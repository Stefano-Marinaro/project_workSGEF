using GoCare.Data;
using GoCare.Models.Domain;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Provisioning;

public sealed class ProfileProvisioningService(GoCareDbContext db)
{
    public async Task CreatePersonSkeletonAsync(Guid accountId, string email, CancellationToken ct)
    {
        var person = new Person(accountId, email);
        db.Persons.Add(person);
        await db.SaveChangesAsync(ct);
    }

    public async Task CompletePersonProfileAsync(Guid accountId, PersonProfileData data, CancellationToken ct)
    {
        var person = await db.Persons.SingleAsync(p => p.Id == accountId, ct);
        person.CompleteProfile(data.Name, data.Surname, data.BirthDate, data.Phone);
        await db.SaveChangesAsync(ct);
    }

    public async Task CreateAssociationSkeletonAsync(Guid accountId, string email, CancellationToken ct)
    {
        var association = new Association(accountId, email);
        db.Associations.Add(association);
        await db.SaveChangesAsync(ct);
    }

    public async Task CompleteAssociationProfileAsync(Guid accountId, AssociationProfileData data, CancellationToken ct)
    {
        var association = await db.Associations.SingleAsync(a => a.Id == accountId, ct);
        association.CompleteProfile(data.Name, data.Headquarter, data.Phones, data.CoveredProvinces);
        await db.SaveChangesAsync(ct);
    }
}
