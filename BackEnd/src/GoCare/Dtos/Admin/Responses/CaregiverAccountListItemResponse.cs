using GoCare.Models.Enums;

namespace GoCare.Dtos.Admin.Responses;

// una riga della ricerca caregiver vista dall'admin
public sealed record CaregiverAccountListItemResponse(
    Guid Id,
    string? Name,
    string? Surname,
    string Email,
    EAccountStatus AccountStatus,
    DateTimeOffset CreatedAt);
