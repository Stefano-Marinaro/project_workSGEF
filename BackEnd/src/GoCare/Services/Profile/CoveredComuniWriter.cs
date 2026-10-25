using GoCare.Data;
using GoCare.Models.Domain;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Profile;

// Allinea i comuni coperti di un'associazione alla nuova lista.
// Non salva: ci pensa la SaveChangesAsync del chiamante.
public static class CoveredComuniWriter
{
    public static async Task ReplaceAsync(GoCareDbContext db, Guid associationId, List<string> codes, CancellationToken ct)
    {
        var current = await db.AssociationCoveredComuni
            .Where(c => c.AssociationId == associationId)
            .ToListAsync(ct);

        // tolgo quelli che non ci sono più
        db.AssociationCoveredComuni.RemoveRange(current.Where(c => !codes.Contains(c.ComuneCodiceIstat)));

        // aggiungo solo quelli nuovi
        var existing = current.Select(c => c.ComuneCodiceIstat).ToHashSet();
        db.AssociationCoveredComuni.AddRange(codes
            .Where(code => !existing.Contains(code))
            .Select(code => new AssociationCoveredComune(associationId, code)));
    }
}