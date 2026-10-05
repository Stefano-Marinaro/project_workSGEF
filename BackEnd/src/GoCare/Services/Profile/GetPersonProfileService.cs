using GoCare.Data;
using GoCare.Dtos.Domain.Responses;
using GoCare.Errors;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Profile;

public sealed class GetPersonProfileService(GoCareDbContext db)
{
    public async Task<PersonProfileResponse> GetAsync(Guid accountId, CancellationToken ct)
    {
        var person = await db.Persons.AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == accountId && p.DeletedAt == null, ct)
            ?? throw new NotFoundException("Profilo caregiver non trovato.");

        return new PersonProfileResponse(
            person.Name,
            person.Surname,
            person.BirthDate,
            person.Phone,
            person.HomeAddress,
            person.Email);
    }
}
