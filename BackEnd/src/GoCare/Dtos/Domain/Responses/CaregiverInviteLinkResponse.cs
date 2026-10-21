namespace GoCare.Dtos.Domain.Responses;

public sealed record CaregiverInviteLinkResponse(string Url, DateTimeOffset ExpiresAt);
