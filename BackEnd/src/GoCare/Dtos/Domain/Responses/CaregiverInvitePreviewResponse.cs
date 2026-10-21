namespace GoCare.Dtos.Domain.Responses;

public sealed record CaregiverInvitePreviewResponse(
    string AssistedPersonName,
    string AssistedPersonSurname,
    string? InvitedByName,
    string? InvitedBySurname,
    bool AlreadyLinked);
