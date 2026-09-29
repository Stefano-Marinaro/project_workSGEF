namespace GoCare.Dtos.Auth.Requests;

public sealed record ChangeEmailRequest(
    string NewEmail,
    string CurrentPassword);
