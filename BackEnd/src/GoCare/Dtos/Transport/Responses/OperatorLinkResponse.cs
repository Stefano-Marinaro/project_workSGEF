namespace GoCare.Dtos.Transport.Responses;

public sealed record OperatorLinkResponse(string Url, DateTimeOffset ExpiresAt);
