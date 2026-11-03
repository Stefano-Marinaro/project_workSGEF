namespace GoCare.Services.Transport;

public sealed class CoverageOptions
{
    public const string SectionName = "Coverage";

    // PA-01: sotto questa soglia rispetto alla partenza, una richiesta ancora "in attesa"
    // e non accettata da nessuna associazione passa a "non coperta" (CoverageTimeoutJob).
    public int NotCoveredThresholdHours { get; init; }
}
