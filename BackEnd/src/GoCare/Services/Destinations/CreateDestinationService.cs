using GoCare.Data;
using GoCare.Dtos.Destinations.Responses;
using GoCare.Models.Domain;

namespace GoCare.Services.Destinations;

public sealed class CreateDestinationService(GoCareDbContext db)
{
    public async Task<SavedDestinationResponse> CreateAsync(
        Guid personId, string placeName, Address savedAddress, string? note, CancellationToken ct)
    {
        var destinationId = Guid.NewGuid();
        var destination = new SavedDestination(destinationId, personId, placeName, savedAddress, note);

        db.SavedDestinations.Add(destination);
        await db.SaveChangesAsync(ct);

        return new SavedDestinationResponse(destinationId, placeName, savedAddress, note);
    }
}
