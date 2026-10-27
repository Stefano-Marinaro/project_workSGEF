using GoCare.Data;
using GoCare.Dtos.Admin.Responses;
using GoCare.Errors;
using GoCare.Models.Enums;
using GoCare.Pagination;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Admin;

// storico delle richieste in cui l'associazione era candidata, con esito e indicatori (UC 11.3 · AD-03)
public sealed class PartnerAssociationRequestsService(GoCareDbContext db)
{
    public async Task<PartnerAssociationRequestsResponse> ListAsync(
        Guid associationId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        ECandidateOutcome? outcome,
        PageQuery page,
        CancellationToken ct)
    {
        var exists = await db.Associations.AnyAsync(a => a.Id == associationId, ct);
        if (!exists)
            throw new NotFoundException("Associazione non trovata.");

        var query =
            from c in db.TransportRequestCandidates.AsNoTracking()
            join t in db.TransportRequests.AsNoTracking() on c.TransportRequestId equals t.Id
            where c.AssociationId == associationId
            select t;

        // periodo sulla data di creazione della richiesta
        if (from is not null)
            query = query.Where(t => t.CreatedAt >= from);
        if (to is not null)
            query = query.Where(t => t.CreatedAt <= to);

        var rows = await query
            .Select(t => new
            {
                t.Id, t.CreatedAt, t.DepartureDateHour, t.TripType, StartCity = t.StartAddress.City,
                t.RequestStatus, t.AssignedAssociationId, t.AcceptedAt
            })
            .ToListAsync(ct);

        var ids = rows.Select(r => r.Id).ToList();

        // l'ultimo rifiuto o disdetta di questa associazione per ogni richiesta
        var rejections = (await db.TransportRequestRejections.AsNoTracking()
                .Where(r => r.AssociationId == associationId && ids.Contains(r.TransportRequestId))
                .ToListAsync(ct))
            .GroupBy(r => r.TransportRequestId)
            .ToDictionary(g => g.Key, g => g.MaxBy(r => r.RejectedAt)!);

        // l'esito dipende da più tabelle: con i volumi di un'associazione si calcola in memoria
        var items = rows.Select(r =>
        {
            rejections.TryGetValue(r.Id, out var rejection);
            var itemOutcome = rejection?.Kind switch
            {
                ERejectionKind.Declined => ECandidateOutcome.Declined,
                ERejectionKind.CancelledAfterAcceptance => ECandidateOutcome.CancelledAfterAcceptance,
                _ when r.RequestStatus == ETripRequestStatus.Cancelled => ECandidateOutcome.Cancelled,
                _ when r.AssignedAssociationId == associationId => ECandidateOutcome.Accepted,
                _ => r.RequestStatus switch
                {
                    ETripRequestStatus.Pending => ECandidateOutcome.Pending,
                    ETripRequestStatus.NotCovered => ECandidateOutcome.NotCovered,
                    _ => ECandidateOutcome.TakenByAnother
                }
            };

            return (Item: new PartnerAssociationRequestItem(
                    r.Id, r.CreatedAt, r.DepartureDateHour, r.TripType, r.StartCity, r.RequestStatus,
                    itemOutcome, rejection?.Reason),
                AcceptanceMinutes: itemOutcome == ECandidateOutcome.Accepted && r.AcceptedAt is not null
                    ? (r.AcceptedAt.Value - r.CreatedAt).TotalMinutes
                    : (double?)null);
        }).ToList();

        var acceptanceTimes = items.Where(i => i.AcceptanceMinutes is not null).Select(i => i.AcceptanceMinutes!.Value).ToList();

        var indicators = new PartnerAssociationIndicators(
            Received: items.Count,
            Accepted: items.Count(i => i.Item.Outcome is ECandidateOutcome.Accepted or ECandidateOutcome.CancelledAfterAcceptance),
            Declined: items.Count(i => i.Item.Outcome == ECandidateOutcome.Declined),
            CancelledAfterAcceptance: items.Count(i => i.Item.Outcome == ECandidateOutcome.CancelledAfterAcceptance),
            AverageAcceptanceMinutes: acceptanceTimes.Count > 0 ? Math.Round(acceptanceTimes.Average(), 1) : null);

        var filtered = items
            .Select(i => i.Item)
            .Where(i => outcome is null || i.Outcome == outcome)
            .OrderByDescending(i => i.CreatedAt)
            .ToList();

        var pageItems = filtered.Skip(page.Skip).Take(page.PageSize).ToList();

        return new PartnerAssociationRequestsResponse(
            indicators,
            new PagedResult<PartnerAssociationRequestItem>(pageItems, filtered.Count, page.Page, page.PageSize));
    }
}
