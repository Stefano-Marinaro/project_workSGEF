using GoCare.Models.Domain;

namespace GoCare.Abstractions;

public interface IGeocodingService
{
    Task<IReadOnlyList<AddressSuggestion>> SearchAsync(string query, CancellationToken ct = default);
}
