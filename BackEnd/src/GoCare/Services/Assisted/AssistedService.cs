using GoCare.Data;
using GoCare.Models.Domain;

namespace GoCare.Services.Assisted;

public sealed class AssistedService(GoCareDbContext db)
{
    // v0: collegamento diretto e subito attivo, nessuna accettazione richiesta.
    public async Task<Guid> CreateAsync(
        Guid caregiverId, string name, string surname, DateOnly birthDate, string phone, CancellationToken ct)
    {
        var assistedId = Guid.NewGuid();

        var assisted = new AssistedPerson(assistedId, name, surname, birthDate, phone, createdBy: caregiverId);
        var link = new CaregiverAssistedLink(caregiverId, assistedId, DateTimeOffset.UtcNow);

        db.AssistedPeople.Add(assisted);
        db.CaregiverAssistedLinks.Add(link);

        await db.SaveChangesAsync(ct);

        return assistedId;
    }
}
