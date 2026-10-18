using GoCare.Dtos.Transport.Requests;

namespace GoCare.Dtos.Domain.Requests;

public sealed record UpdateAssociationProfileRequest(
    string Name,
    AddressRequest Headquarter,
    List<string> Phones,
    List<string> CoveredProvinces,
    string? AvailabilityHours
);