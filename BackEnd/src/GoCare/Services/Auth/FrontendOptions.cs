namespace GoCare.Services.Auth;

public sealed class FrontendOptions
{
    public const string SectionName = "Frontend";

    public string VerifyEmailUrl { get; set; } = null!;
    public string ResetPasswordUrl { get; set; } = null!;
    public string ChangeEmailUrl { get; set; } = null!;
    public string OperatorUrl { get; set; } = null!;
    public string CaregiverInviteUrl { get; set; } = null!;
    public string SetPasswordUrl { get; set; } = null!;
}
