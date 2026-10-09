using GoCare.Models.Domain;

namespace GoCare.Dtos.Destinations.Responses;

public sealed record SavedDestinationResponse(Guid Id, string PlaceName, Address SavedAddress, string? Note);
