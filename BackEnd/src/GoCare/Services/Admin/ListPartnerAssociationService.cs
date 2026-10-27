using GoCare.Data;
using GoCare.Dtos.Admin.Responses;
using GoCare.Models.Enums;
using GoCare.Pagination;
using GoCare.Dtos.Domain.Responses;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Admin;

public sealed class ListPartnerAssociationsService(GoCareDbContext db)
{
    public async Task<PagedResult<PartnerAssociationListItemResponse>> ListAsync(
        string? search, EAccountStatus? status, string? comune, PageQuery page, CancellationToken ct)
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

        if (!string.IsNullOrWhiteSpace(comune))
            query = query.Where(x => db.AssociationCoveredComuni
                .Any(c => c.AssociationId == x.a.Id && c.ComuneCodiceIstat == comune));
        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(x => x.a.Name)
            .Skip(page.Skip)
            .Take(page.PageSize)
            .Select(x => new PartnerAssociationListItemResponse(
                x.a.Id, x.a.Name, x.acc.Email,
                (from c in db.AssociationCoveredComuni
                join co in db.Comuni on c.ComuneCodiceIstat equals co.CodiceIstat
                where c.AssociationId == x.a.Id
                orderby co.NomeComune
                select new CoveredComuneResponse(co.CodiceIstat, co.NomeComune, co.Provincia)).ToList(),
                x.acc.Status,
                // in carico = assegnati a lei e non ancora conclusi
                db.TransportRequests.Count(t => t.AssignedAssociationId == x.a.Id
                    && (t.RequestStatus == ETripRequestStatus.Confirmed || t.RequestStatus == ETripRequestStatus.InProgress))))
            .ToListAsync(ct);

        return new PagedResult<PartnerAssociationListItemResponse>(items, total, page.Page, page.PageSize);
    }
}