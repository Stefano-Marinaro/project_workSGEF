using GoCare.Models.Enums;

namespace GoCare.Dtos.Admin.Responses;

public sealed record CaregiverAccountDetailResponse(
    Guid Id,
    string? Name,
    string? Surname,
    string Email,
    string? Phone,
    DateOnly? BirthDate,
    EAccountStatus AccountStatus,
    DateTimeOffset CreatedAt,
    DateTimeOffset? EmailVerifiedAt,
    DateTimeOffset? SuspendedAt,
    DateTimeOffset? DeletedAt,
    DateTimeOffset? AnonymizedAt,      // valorizzato = dati personali cancellati, non più ripristinabile
    int FailedLoginAttempts,
    int LinkedAssistedPeople,
    int TransportRequests);
