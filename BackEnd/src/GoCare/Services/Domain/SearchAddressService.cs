using GoCare.Abstractions;
using GoCare.Models.Domain;

namespace GoCare.Services.Domain;

public sealed class SearchAddressService(IGeocodingService geocoder)
{
    public async Task<IReadOnlyList<AddressSuggestion>> SearchAsync(string query, CancellationToken ct)
    {
        var normalized = query?.Trim() ?? "";
        if (normalized.Length < 3)
            return [];

        try
        {
            return await geocoder.SearchAsync(normalized, ct);
        }
        catch (Exception)
        {
            // Nominatim e' un servizio esterno vero: puo' essere lento, giu', o al limite di
            // 1 richiesta/secondo. Un suo intoppo non deve rompere la UI con un 500 - meglio
            // nessun risultato, come se la ricerca non avesse trovato nulla.
            return [];
        }
    }
}
