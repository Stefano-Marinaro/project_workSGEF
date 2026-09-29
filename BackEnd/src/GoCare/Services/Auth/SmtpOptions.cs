namespace GoCare.Services.Auth;

public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";

    public string Host { get; init; } = null!;
    public int Port { get; init; }
    public string Username { get; init; } = null!;
    public string Password { get; init; } = null!;
    public string FromAddress { get; init; } = "no-reply@gocare.it";
    public string FromName { get; init; } = "GoCare";
}
