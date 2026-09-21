namespace GoCare.Dtos.Auth.Responses;

public sealed record AuthTokenResponse(string AccessToken, string RefreshToken);
