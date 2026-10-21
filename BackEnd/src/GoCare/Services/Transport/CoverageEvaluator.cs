using GoCare.Data;
using GoCare.Models.Domain;
using GoCare.Models.Enums;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Transport;

// decide quando una richiesta in attesa diventa NotCovered.
// Per ora una sola regola: tutte le candidate hanno rifiutato. La soglia temporale (PA-02) andrà qui.
public sealed class CoverageEvaluator(GoCareDbContext db)
{
    public async Task<bool> IsNotCoveredAsync(TransportRequest transport, CancellationToken ct)
    {
        if (transport.RequestStatus is not ETripRequestStatus.Pending)
            return false;

        // candidate che non hanno ancora un rifiuto registrato (né salvato né in attesa di SaveChanges)
        var candidates = await db.TransportRequestCandidates
            .Where(c => c.TransportRequestId == transport.Id)
            .Select(c => c.AssociationId)
            .ToListAsync(ct);

        var rejectedSaved = await db.TransportRequestRejections
            .Where(r => r.TransportRequestId == transport.Id)
            .Select(r => r.AssociationId)
            .ToListAsync(ct);

        var rejectedPending = db.ChangeTracker.Entries<TransportRequestRejection>()
            .Where(e => e.State == EntityState.Added && e.Entity.TransportRequestId == transport.Id)
            .Select(e => e.Entity.AssociationId);

        var rejected = rejectedSaved.Concat(rejectedPending).ToHashSet();

        return candidates.All(rejected.Contains);
    }
}
