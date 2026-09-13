using GoCare.Application.Data;
using GoCare.Application.Models.Domain;
using GoCare.Application.Services.Auth;

namespace GoCare.Application.Services.Provisioning;

public sealed class ProfileProvisioningService(BusinessDbContext db) : IProfileProvisioningService
{
    public async Task CreatePersonAsync(Guid accountId, PersonProvisioningData data, CancellationToken ct)
    {
        var person = new Person(accountId, data.Name, data.Surname, data.BirthDate, data.Email, data.Phone);
        db.Persons.Add(person);
        await db.SaveChangesAsync(ct);
    }

    public async Task CreateAssociationAsync(Guid accountId, AssociationProvisioningData data, CancellationToken ct)
    {
        var association = new Association(accountId, data.Name, data.Headquarter, data.Phones, data.Email, data.CoveredProvinces);
        db.Associations.Add(association);
        await db.SaveChangesAsync(ct);  
    }
}
