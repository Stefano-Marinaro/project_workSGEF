using GoCare.Models.Domain;
using GoCare.Models.Enums;

namespace GoCare.Dtos.Transport.Responses;

public sealed record AssociationTransportResponse(
    Guid Id,
    ETripType TripType,
    ETripDirection TripDirection,
    DateTimeOffset DepartureDateHour,
    DateTimeOffset? ReturnDateHour,
    Address StartAddress,
    Address EndAddress,
    ETripRequestStatus RequestStatus);
