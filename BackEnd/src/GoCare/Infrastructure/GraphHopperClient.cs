using System.Globalization;
using System.Net.Http.Json;

using GoCare.Abstractions;

using Microsoft.Extensions.Options;

namespace GoCare.Infrastructure;

public sealed class GraphHopperClient(HttpClient httpClient, IOptions<GraphHopperOptions> options) : IRoutingService
{
    public async Task<double> GetDistanceKmAsync(
        double startLatitude, double startLongitude,
        double endLatitude, double endLongitude,
        CancellationToken ct = default)
    {
        // GraphHopper vuole le coordinate come "lat,lon" - al contrario di OpenRouteService,
        // che invece vuole "lon,lat". Facile confondersi passando da uno all'altro.
        var start = FormatCoordinate(startLatitude, startLongitude);
        var end = FormatCoordinate(endLatitude, endLongitude);

        var url = "https://graphhopper.com/api/1/route" +
                   $"?point={start}&point={end}&vehicle=car&key={Uri.EscapeDataString(options.Value.ApiKey)}";

        using var response = await httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GraphHopperResponse>(cancellationToken: ct);
        var distanceMeters = result?.Paths?.FirstOrDefault()?.Distance
            ?? throw new InvalidOperationException("Impossibile calcolare il percorso tra gli indirizzi forniti.");

        return distanceMeters / 1000.0;
    }

    private static string FormatCoordinate(double latitude, double longitude) =>
        string.Create(CultureInfo.InvariantCulture, $"{latitude},{longitude}");

    private sealed record GraphHopperResponse(GraphHopperPath[]? Paths);
    private sealed record GraphHopperPath(double Distance);
}
