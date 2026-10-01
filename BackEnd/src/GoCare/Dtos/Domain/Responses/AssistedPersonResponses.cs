using GoCare.Models.Enums;

namespace GoCare.Dtos.Domain.Responses;

public sealed record AddressResponse(
    string Street,
    string Number,
    string PostalCode,
    string City,
    string Province);

public sealed record AssistedPersonSummaryResponse(
    Guid Id,
    string Name,
    string Surname,
    DateOnly BirthDate,
    string Phone,
    AddressResponse? HomeAddress,
    AssistedTripResponse? NextTrip);

public sealed record AssistedPersonDetailResponse(
    Guid Id,
    string Name,
    string Surname,
    DateOnly BirthDate,
    string Phone,
    AddressResponse? HomeAddress,
    IReadOnlyList<CaregiverResponse> Caregivers,
    IReadOnlyList<AssistedTripResponse> Trips);

public sealed record CaregiverResponse(
    Guid Id,
    string? Name,
    string? Surname,
    string Email);

public sealed record AssistedTripResponse(
    Guid Id,
    Guid RequestedById,
    ETripType TripType,
    ETripDirection TripDirection,
    DateTimeOffset DepartureDateHour,
    DateTimeOffset? ReturnDateHour,
    ETripRequestStatus Status);
