namespace GoCare.Services.Payments;

public sealed class StripeOptions
{
    public const string SectionName = "Stripe";

    // Segreti: vanno in appsettings.Local.json (mai committato), come JWT/SMTP/OpenRouteService.
    public string SecretKey { get; init; } = null!;
    public string WebhookSecret { get; init; } = null!;
}
