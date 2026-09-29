using GoCare.Data;
using GoCare.Errors;
using GoCare.Models.Domain;
using GoCare.Models.Enums;

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
        var person = await db.Persons.SingleOrDefaultAsync(p => p.Id == accountId, ct)
            ?? throw new NotFoundException("Profilo caregiver non trovato.");

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
        var association = await db.Associations.SingleOrDefaultAsync(a => a.Id == accountId, ct)
            ?? throw new NotFoundException("Profilo associazione non trovato.");

        association.CompleteProfile(data.Name, data.Headquarter, data.Phones, data.CoveredProvinces);
        await db.SaveChangesAsync(ct);
    }

    public async Task AccreditAssociationAsync(Guid associationId, CancellationToken ct)
    {
        var association = await db.Associations.SingleOrDefaultAsync(a => a.Id == associationId, ct)
            ?? throw new NotFoundException("Associazione non trovata.");

        if (!association.IsProfileComplete)
            throw new ConflictException("Il profilo deve essere completato prima dell'accreditamento.");

        if (association.Status is not EAccreditationStatus.Pending)
            throw new ConflictException("Solo un'associazione in attesa può essere accreditata.");

        association.Accredit();
        await db.SaveChangesAsync(ct);
    }

    public async Task RejectAssociationAsync(Guid associationId, CancellationToken ct)
    {
        var association = await db.Associations.SingleOrDefaultAsync(a => a.Id == associationId, ct)
            ?? throw new NotFoundException("Associazione non trovata.");

        if (association.Status is not EAccreditationStatus.Pending)
            throw new ConflictException("Solo un'associazione in attesa può essere rifiutata.");

        association.Reject();
        await db.SaveChangesAsync(ct);
    }
}
