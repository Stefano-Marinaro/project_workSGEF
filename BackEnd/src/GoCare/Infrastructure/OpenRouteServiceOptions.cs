namespace GoCare.Infrastructure;

public sealed class OpenRouteServiceOptions
{
    public const string SectionName = "OpenRouteService";

    // Chiave gratuita, un account su openrouteservice.org, nessuna carta di credito.
    // Va in appsettings.Local.json (mai committato), come le altre chiavi/segreti.
    public string ApiKey { get; init; } = null!;
}
