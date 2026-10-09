using GoCare.Dtos.Transport.Requests;

namespace GoCare.Dtos.Destinations.Requests;

public sealed record SavedDestinationRequest(string PlaceName, AddressRequest SavedAddress, string? Note);
