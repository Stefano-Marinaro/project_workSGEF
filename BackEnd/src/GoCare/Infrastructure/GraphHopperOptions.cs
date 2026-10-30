namespace GoCare.Infrastructure;

public sealed class GraphHopperOptions
{
    public const string SectionName = "GraphHopper";

    // Chiave gratuita, un account su graphhopper.com, nessuna carta di credito.
    // Va in appsettings.Local.json (mai committato).
    public string ApiKey { get; init; } = null!;
}
