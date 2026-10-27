using GoCare.Dtos.Domain.Responses;
using GoCare.Models.Domain;
using GoCare.Models.Enums;

namespace GoCare.Dtos.Admin.Responses;

public sealed record PartnerAssociationDetailResponse(
    Guid Id,
    string? Name,
    string Email,
    Address? Headquarter,
    List<string>? Phones,
    List<CoveredComuneResponse> CoveredComuni,
    string? AvailabilityHours,
    EAccountStatus AccountStatus,          // Unverified = invito non ancora accettato
    DateTimeOffset CreatedAt,
    DateTimeOffset? EmailVerifiedAt,
    DateTimeOffset? DeletedAt,
    List<PartnerAssociationTransportResponse> ActiveTransports,   // Confirmed e InProgress
    List<PartnerAssociationTransportResponse> RecentTransports);  // gli ultimi conclusi, annullati o non coperti
