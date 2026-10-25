using GoCare.Data;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Domain;

// Cerca i comuni ISTAT per nome (anche parziale) o per CAP, al massimo 10 risultati.
// Restituisce il codice ISTAT, che è il valore da inviare in coveredComuni (PA-03).

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
            .Select(c => new ComuneSuggestion(c.CodiceIstat, c.NomeComune, c.Provincia, c.CapCodes))
            .ToListAsync(ct);
    }
}

public sealed record ComuneSuggestion(string CodiceIstat, string NomeComune, string Provincia, List<string> CapCodes);
