using GoCare.Models.Domain;
using GoCare.Models.Enums;

namespace GoCare.Dtos.Transport.Responses;

public sealed record TripDetailResponse(
                            ETripType TripType,
                            ETripDirection TripDirection,
                            DateTimeOffset DepartureDateHour,
                            DateTimeOffset? ReturnDateHour,
                            TripAddress StartAddress,
                            TripAddress EndAddress,
                            TripAddress? ReturnEndAddress,
                            ETripRequestStatus RequestStatus,
                            string? AssociationName,
                            List<string>? AssociationPhones);

