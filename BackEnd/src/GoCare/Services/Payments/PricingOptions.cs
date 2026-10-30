namespace GoCare.Services.Payments;

public sealed class PricingOptions
{
    public const string SectionName = "Pricing";

    public decimal PricePerKm { get; init; }
    public decimal MinimumAmount { get; init; }
}
