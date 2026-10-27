namespace GoCare.Dtos.Auth.Requests;

public sealed record SetPasswordRequest(string Token, string Password);
