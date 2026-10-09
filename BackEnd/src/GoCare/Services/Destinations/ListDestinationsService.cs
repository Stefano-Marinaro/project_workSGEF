using GoCare.Data;
using GoCare.Dtos.Destinations.Responses;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Destinations;

public sealed class ListDestinationsService(GoCareDbContext db)
{
    public async Task<List<SavedDestinationResponse>> ListAsync(Guid personId, CancellationToken ct)
    {
        return await db.SavedDestinations
            .Where(d => d.PersonId == personId)
            .OrderBy(d => d.PlaceName)
            .Select(d => new SavedDestinationResponse(d.Id, d.PlaceName, d.SavedAddress, d.Note))
            .ToListAsync(ct);
    }
}
