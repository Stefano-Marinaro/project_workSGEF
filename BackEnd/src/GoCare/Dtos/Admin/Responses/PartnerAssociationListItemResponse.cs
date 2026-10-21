using GoCare.Models.Enums;

namespace GoCare.Dtos.Admin.Responses;

// una riga dell'elenco associazioni visto dall'admin
public sealed record PartnerAssociationListItemResponse(
    Guid Id,
    string? Name,
    string Email,
    List<string>? CoveredProvinces,
    EAccountStatus AccountStatus,
    int ActiveTransports);