using GoCare.Models.Enums;

namespace GoCare.Dtos.Transport.Requests;

public sealed record NewTransportRequest(
    Guid BeneficiaryId,
    ETripType TripType,
    ETripDirection TripDirection,
    DateTimeOffset DepartureDateHour,
    DateTimeOffset? ReturnDateHour,
    TripAddressRequest StartAddress,
    TripAddressRequest EndAddress,
    TripAddressRequest? ReturnEndAddress,
    string ReferencePhone,
    string? ReferenceEmail,
    IReadOnlyList<CompanionRequest>? Companions);
