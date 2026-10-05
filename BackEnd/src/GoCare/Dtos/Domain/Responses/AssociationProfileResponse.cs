using GoCare.Models.Domain;
using GoCare.Models.Enums;

namespace GoCare.Dtos.Domain.Responses;

public sealed record AssociationProfileResponse(
    string? Name,
    Address? Headquarter,
    List<string>? Phones,
    List<string>? CoveredProvinces,
    string? AvailabilityHours,
    string Email,
    EAccreditationStatus Status);
