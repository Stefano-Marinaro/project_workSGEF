using GoCare.Data;
using GoCare.Dtos.Domain.Responses;
using GoCare.Errors;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Profile;

public sealed class GetAssociationProfileService(GoCareDbContext db)
{
    public async Task<AssociationProfileResponse> GetAsync(Guid accountId, CancellationToken ct)
    {
        var association = await db.Associations.AsNoTracking()
            .SingleOrDefaultAsync(a => a.Id == accountId && a.DeletedAt == null, ct)
            ?? throw new NotFoundException("Profilo associazione non trovato.");

        var coveredComuni = await (
            from c in db.AssociationCoveredComuni
            join comune in db.Comuni on c.ComuneCodiceIstat equals comune.CodiceIstat
            where c.AssociationId == accountId
            orderby comune.NomeComune
            select new CoveredComuneResponse(comune.CodiceIstat, comune.NomeComune, comune.Provincia))
            .ToListAsync(ct);

        return new AssociationProfileResponse(
            association.Name,
            association.Headquarter,
            association.Phones,
            coveredComuni,
            association.AvailabilityHours,
            association.Email,
            association.Status);
    }
}
