using GoCare.Data;
using GoCare.Errors;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Destinations;

public sealed class DeleteDestinationService(GoCareDbContext db)
{
    public async Task DeleteAsync(Guid personId, Guid destinationId, CancellationToken ct)
    {
        var destination = await db.SavedDestinations.SingleOrDefaultAsync(
            d => d.Id == destinationId && d.PersonId == personId, ct)
            ?? throw new NotFoundException("Destinazione non trovata.");

        db.SavedDestinations.Remove(destination);

        await db.SaveChangesAsync(ct);
    }
}
