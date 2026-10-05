using GoCare.Data;
using GoCare.Errors;
using GoCare.Models.Domain;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Profile;

public sealed class UpdatePersonProfileService(GoCareDbContext db)
{
    public async Task UpdateAsync(
        Guid accountId,
        string name,
        string surname,
        DateOnly birthDate,
        string phone,
        Address? homeAddress,
        CancellationToken ct)
    {
        var person = await db.Persons.SingleOrDefaultAsync(p => p.Id == accountId && p.DeletedAt == null, ct)
            ?? throw new NotFoundException("Profilo caregiver non trovato.");

        person.UpdateProfile(name, surname, birthDate, phone, homeAddress);

        await db.SaveChangesAsync(ct);
    }
}
