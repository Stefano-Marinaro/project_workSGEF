using GoCare.Data;
using GoCare.Dtos.Admin.Responses;
using GoCare.Models.Enums;
using GoCare.Pagination;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Admin;

// ricerca degli account caregiver per l'assistenza (UC 11.5 · AD-05)
public sealed class ListCaregiverAccountsService(GoCareDbContext db)
{
    public async Task<PagedResult<CaregiverAccountListItemResponse>> ListAsync(
        string? search, EAccountStatus? status, PageQuery page, CancellationToken ct)
    {
        // persona e account hanno lo stesso Id
        var query =
            from p in db.Persons.AsNoTracking()
            join acc in db.Accounts.AsNoTracking() on p.Id equals acc.Id
            where acc.Role == EAccountRole.Person
            select new { p, acc };

        // la ricerca guarda email, nome e cognome
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => EF.Functions.ILike(x.acc.Email, $"%{search}%")
                || (x.p.Name != null && EF.Functions.ILike(x.p.Name, $"%{search}%"))
                || (x.p.Surname != null && EF.Functions.ILike(x.p.Surname, $"%{search}%")));

        if (status is not null)
            query = query.Where(x => x.acc.Status == status);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(x => x.p.Surname).ThenBy(x => x.p.Name).ThenBy(x => x.acc.Email)
            .Skip(page.Skip)
            .Take(page.PageSize)
            .Select(x => new CaregiverAccountListItemResponse(
                x.p.Id, x.p.Name, x.p.Surname, x.acc.Email, x.acc.Status, x.acc.CreatedAt))
            .ToListAsync(ct);

        return new PagedResult<CaregiverAccountListItemResponse>(items, total, page.Page, page.PageSize);
    }
}
