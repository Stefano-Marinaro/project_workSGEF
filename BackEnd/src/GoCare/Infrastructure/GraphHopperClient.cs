using System.Globalization;
using System.Net.Http.Json;

using GoCare.Abstractions;
using GoCare.Errors;

using Microsoft.Extensions.Options;

namespace GoCare.Infrastructure;

public sealed class GraphHopperClient(HttpClient httpClient, IOptions<GraphHopperOptions> options) : IRoutingService
{
    public async Task<double> GetDistanceKmAsync(
        double startLatitude, double startLongitude,
        double endLatitude, double endLongitude,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(options.Value.ApiKey))
            throw new ServiceUnavailableException("Il servizio di calcolo del percorso non è configurato. Contatta il gestore dell’app.");
        // GraphHopper vuole le coordinate come "lat,lon" - al contrario di OpenRouteService,
        // che invece vuole "lon,lat". Facile confondersi passando da uno all'altro.
        var start = FormatCoordinate(startLatitude, startLongitude);
        var end = FormatCoordinate(endLatitude, endLongitude);

        var url = "https://graphhopper.com/api/1/route" +
                   $"?point={start}&point={end}&vehicle=car&key={Uri.EscapeDataString(options.Value.ApiKey)}";

        HttpResponseMessage response;
        try
        {
            response = await httpClient.GetAsync(url, ct);
        }
        catch (HttpRequestException)
        {
            throw new ServiceUnavailableException("Impossibile raggiungere il servizio di calcolo del percorso. Riprova più tardi.");
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new ServiceUnavailableException("Il calcolo del percorso ha impiegato troppo tempo. Riprova.");
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
                throw new ServiceUnavailableException("Il servizio di calcolo del percorso non è disponibile. Riprova più tardi o contatta il gestore dell’app.");
            var result = await response.Content.ReadFromJsonAsync<GraphHopperResponse>(cancellationToken: ct);
            var distanceMeters = result?.Paths?.FirstOrDefault()?.Distance
                ?? throw new ServiceUnavailableException("Impossibile calcolare il percorso tra gli indirizzi forniti.");

            return distanceMeters / 1000.0;
        }
    }

    private static string FormatCoordinate(double latitude, double longitude) =>
        string.Create(CultureInfo.InvariantCulture, $"{latitude},{longitude}");

    private sealed record GraphHopperResponse(GraphHopperPath[]? Paths);
    private sealed record GraphHopperPath(double Distance);
}
