using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

using GoCare.Abstractions;

using Microsoft.Extensions.Options;

namespace GoCare.Infrastructure;

public sealed class OpenRouteServiceClient(HttpClient httpClient, IOptions<OpenRouteServiceOptions> options) : IRoutingService
{
    public async Task<double> GetDistanceKmAsync(
        double startLatitude, double startLongitude,
        double endLatitude, double endLongitude,
        CancellationToken ct = default)
    {
        // ORS vuole le coordinate come "lon,lat" (longitudine prima), al contrario di come
        // di solito si scrivono lat/lon - errore facile da fare, qui è esplicito nel codice.
        var start = FormatCoordinate(startLongitude, startLatitude);
        var end = FormatCoordinate(endLongitude, endLatitude);

        var url = "https://api.openrouteservice.org/v2/directions/driving-car" +
                   $"?api_key={Uri.EscapeDataString(options.Value.ApiKey)}&start={start}&end={end}";

        using var response = await httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OrsResponse>(cancellationToken: ct);
        var distanceMeters = result?.Features?.FirstOrDefault()?.Properties?.Summary?.Distance
            ?? throw new InvalidOperationException("Impossibile calcolare il percorso tra gli indirizzi forniti.");

        return distanceMeters / 1000.0;
    }

    private static string FormatCoordinate(double longitude, double latitude) =>
        string.Create(CultureInfo.InvariantCulture, $"{longitude},{latitude}");

    private sealed record OrsResponse(OrsFeature[]? Features);
    private sealed record OrsFeature(OrsProperties? Properties);
    private sealed record OrsProperties(OrsSummary? Summary);
    private sealed record OrsSummary([property: JsonPropertyName("distance")] double Distance);
}
