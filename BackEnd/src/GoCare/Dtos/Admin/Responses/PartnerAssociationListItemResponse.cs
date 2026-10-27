using GoCare.Models.Enums;
using GoCare.Dtos.Domain.Responses;

namespace GoCare.Dtos.Admin.Responses;

// una riga dell'elenco associazioni visto dall'admin
public sealed record PartnerAssociationListItemResponse(
    Guid Id,
    string? Name,
    string Email,
    List<CoveredComuneResponse> CoveredComuni,
    EAccountStatus AccountStatus,
    int ActiveTransports);