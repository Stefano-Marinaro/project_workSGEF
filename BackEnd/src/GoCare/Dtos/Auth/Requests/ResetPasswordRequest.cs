namespace GoCare.Dtos.Auth.Requests;

public sealed record ResetPasswordRequest(string Token, string NewPassword);
