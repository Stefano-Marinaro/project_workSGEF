namespace GoCare.Services.Auth;

public sealed class FrontendOptions
{
    public const string SectionName = "Frontend";

    public string VerifyEmailUrl { get; set; } = null!;
    public string ResetPasswordUrl { get; set; } = null!;
}
