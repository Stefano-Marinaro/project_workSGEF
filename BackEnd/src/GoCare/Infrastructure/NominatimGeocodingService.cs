using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

using GoCare.Abstractions;
using GoCare.Models.Domain;

namespace GoCare.Infrastructure;

public sealed class NominatimGeocodingService(HttpClient httpClient) : IGeocodingService
{
    public async Task<IReadOnlyList<AddressSuggestion>> SearchAsync(string query, CancellationToken ct = default)
    {
        var url = "https://nominatim.openstreetmap.org/search" +
                   $"?format=json&addressdetails=1&limit=5&countrycodes=it&q={Uri.EscapeDataString(query)}";

        using var response = await httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var results = await response.Content.ReadFromJsonAsync<NominatimResult[]>(cancellationToken: ct) ?? [];
        return results.Select(ToSuggestion).ToList();
    }

    private static AddressSuggestion ToSuggestion(NominatimResult r) => new(
        r.DisplayName,
        r.Address?.Road ?? "",
        r.Address?.HouseNumber ?? "",
        r.Address?.Postcode ?? "",
        r.Address?.City ?? r.Address?.Town ?? r.Address?.Village ?? "",
        ExtractProvinceCode(r.Address?.IsoCode),
        double.Parse(r.Lat, CultureInfo.InvariantCulture),
        double.Parse(r.Lon, CultureInfo.InvariantCulture));

    // Nominatim torna "IT-PG" in ISO3166-2-lvl6 per Perugia - ma NON è garantito per tutte le
    // province (provincie autonome, città metropolitane, Sardegna riordinata): va verificato
    // con richieste reali prima di fidarsene, come segnalato durante la progettazione.
    private static string ExtractProvinceCode(string? iso) =>
        iso?.StartsWith("IT-", StringComparison.OrdinalIgnoreCase) == true && iso.Length == 5
            ? iso[3..].ToUpperInvariant()
            : "";

    private sealed record NominatimResult(
        [property: JsonPropertyName("display_name")] string DisplayName,
        string Lat,
        string Lon,
        NominatimAddress? Address);

    private sealed record NominatimAddress(
        string? Road,
        [property: JsonPropertyName("house_number")] string? HouseNumber,
        string? Postcode,
        string? City,
        string? Town,
        string? Village,
        [property: JsonPropertyName("ISO3166-2-lvl6")] string? IsoCode);
}
