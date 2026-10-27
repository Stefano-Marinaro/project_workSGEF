using GoCare.Data;
using GoCare.Dtos.Admin.Responses;
using GoCare.Dtos.Domain.Responses;
using GoCare.Errors;
using GoCare.Models.Enums;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Admin;

// dettaglio di un'associazione partner per l'admin (UC 11.6 · AD-06). Mostra anche le associazioni eliminate.
public sealed class PartnerAssociationDetailService(GoCareDbContext db)
{
    private const int RecentTransportsLimit = 20;

    public async Task<PartnerAssociationDetailResponse> GetAsync(Guid associationId, CancellationToken ct)
    {
        var row = await (
            from a in db.Associations.AsNoTracking()
            join acc in db.Accounts.AsNoTracking() on a.Id equals acc.Id
            where a.Id == associationId
            select new { a, acc })
            .SingleOrDefaultAsync(ct)
            ?? throw new NotFoundException("Associazione non trovata.");

        var coveredComuni = await (
            from c in db.AssociationCoveredComuni
            join comune in db.Comuni on c.ComuneCodiceIstat equals comune.CodiceIstat
            where c.AssociationId == associationId
            orderby comune.NomeComune
            select new CoveredComuneResponse(comune.CodiceIstat, comune.NomeComune, comune.Provincia))
            .ToListAsync(ct);

        var assigned = db.TransportRequests.AsNoTracking().Where(t => t.AssignedAssociationId == associationId);

        var active = await assigned
            .Where(t => t.RequestStatus == ETripRequestStatus.Confirmed || t.RequestStatus == ETripRequestStatus.InProgress)
            .OrderBy(t => t.DepartureDateHour)
            .Select(t => new PartnerAssociationTransportResponse(
                t.Id, t.TripType, t.TripDirection, t.DepartureDateHour, t.RequestStatus,
                t.StartAddress.City, t.EndAddress.City))
            .ToListAsync(ct);

        var recent = await assigned
            .Where(t => t.RequestStatus != ETripRequestStatus.Confirmed && t.RequestStatus != ETripRequestStatus.InProgress)
            .OrderByDescending(t => t.DepartureDateHour)
            .Take(RecentTransportsLimit)
            .Select(t => new PartnerAssociationTransportResponse(
                t.Id, t.TripType, t.TripDirection, t.DepartureDateHour, t.RequestStatus,
                t.StartAddress.City, t.EndAddress.City))
            .ToListAsync(ct);

        return new PartnerAssociationDetailResponse(
            row.a.Id,
            row.a.Name,
            row.acc.Email,
            row.a.Headquarter,
            row.a.Phones,
            coveredComuni,
            row.a.AvailabilityHours,
            row.acc.Status,
            row.acc.CreatedAt,
            row.acc.EmailVerifiedAt,
            row.a.DeletedAt,
            active,
            recent);
    }
}
