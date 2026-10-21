using GoCare.Data;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Domain;

// Servizio temporaneo di verifica: conferma che l'autofill comune/CAP funzioni sui dati importati.
// Da promuovere a feature vera (con autorizzazione) solo dopo la verifica, non è pensato per restare cosi'.
public sealed class SearchComuneService(GoCareDbContext db)
{
    public async Task<IReadOnlyList<ComuneSuggestion>> SearchAsync(string query, CancellationToken ct)
    {
        var normalized = query?.Trim() ?? "";
        if (normalized.Length < 2)
            return [];

        return await db.Comuni
            .Where(c => EF.Functions.ILike(c.NomeComune, $"%{normalized}%") || c.CapCodes.Contains(normalized))
            .OrderBy(c => c.NomeComune)
            .Take(10)
            .Select(c => new ComuneSuggestion(c.NomeComune, c.Provincia, c.CapCodes))
            .ToListAsync(ct);
    }
}

public sealed record ComuneSuggestion(string NomeComune, string Provincia, List<string> CapCodes);
