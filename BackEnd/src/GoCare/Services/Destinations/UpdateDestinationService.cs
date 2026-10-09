using GoCare.Data;
using GoCare.Errors;
using GoCare.Models.Domain;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Destinations;

public sealed class UpdateDestinationService(GoCareDbContext db)
{
    public async Task UpdateAsync(
        Guid personId, Guid destinationId, string placeName, Address savedAddress, string? note, CancellationToken ct)
    {
        var destination = await db.SavedDestinations.SingleOrDefaultAsync(
            d => d.Id == destinationId && d.PersonId == personId, ct)
            ?? throw new NotFoundException("Destinazione non trovata.");

        destination.UpdateDetails(placeName, savedAddress, note);

        await db.SaveChangesAsync(ct);
    }
}
