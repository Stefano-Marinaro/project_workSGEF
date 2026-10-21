using GoCare.Data;
using GoCare.Dtos.Admin.Responses;
using GoCare.Models.Enums;
using GoCare.Pagination;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Admin;

public sealed class ListPartnerAssociationsService(GoCareDbContext db)
{
    public async Task<PagedResult<PartnerAssociationListItemResponse>> ListAsync(
        string? search, EAccountStatus? status, string? province, PageQuery page, CancellationToken ct)
    {
        // associazione e account hanno lo stesso Id: li uniamo per leggere email e stato dell'account
        var query =
            from a in db.Associations.AsNoTracking()
            join acc in db.Accounts on a.Id equals acc.Id
            select new { a, acc };

        // filtri facoltativi: si applicano solo se il parametro è presente
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.a.Name != null && EF.Functions.ILike(x.a.Name, $"%{search}%"));

        if (status is not null)
            query = query.Where(x => x.acc.Status == status);

        if (!string.IsNullOrWhiteSpace(province))
            query = query.Where(x => x.a.CoveredProvinces != null && x.a.CoveredProvinces.Contains(province.ToUpperInvariant()));

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(x => x.a.Name)
            .Skip(page.Skip)
            .Take(page.PageSize)
            .Select(x => new PartnerAssociationListItemResponse(
                x.a.Id, x.a.Name, x.acc.Email, x.a.CoveredProvinces, x.acc.Status,
                // in carico = assegnati a lei e non ancora conclusi
                db.TransportRequests.Count(t => t.AssignedAssociationId == x.a.Id
                    && (t.RequestStatus == ETripRequestStatus.Confirmed || t.RequestStatus == ETripRequestStatus.InProgress))))
            .ToListAsync(ct);

        return new PagedResult<PartnerAssociationListItemResponse>(items, total, page.Page, page.PageSize);
    }
}