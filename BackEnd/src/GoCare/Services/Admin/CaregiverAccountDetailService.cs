using GoCare.Data;
using GoCare.Dtos.Admin.Responses;
using GoCare.Errors;
using GoCare.Models.Enums;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Admin;

public sealed class CaregiverAccountDetailService(GoCareDbContext db)
{
    public async Task<CaregiverAccountDetailResponse> GetAsync(Guid caregiverId, CancellationToken ct)
    {
        var row = await (
            from p in db.Persons.AsNoTracking()
            join acc in db.Accounts.AsNoTracking() on p.Id equals acc.Id
            where p.Id == caregiverId && acc.Role == EAccountRole.Person
            select new { p, acc })
            .SingleOrDefaultAsync(ct)
            ?? throw new NotFoundException("Caregiver non trovato.");

        var failedLogins = await db.FailedLoginAttempts.CountAsync(f => f.Email == row.acc.Email, ct);
        var linkedAssisted = await db.CaregiverAssistedLinks.CountAsync(l => l.CaregiverId == caregiverId && l.DeletedAt == null, ct);
        var transports = await db.TransportRequests.CountAsync(t => t.RequestedById == caregiverId, ct);

        return new CaregiverAccountDetailResponse(
            row.p.Id,
            row.p.Name,
            row.p.Surname,
            row.acc.Email,
            row.p.Phone,
            row.p.BirthDate,
            row.acc.Status,
            row.acc.CreatedAt,
            row.acc.EmailVerifiedAt,
            row.acc.SuspendedAt,
            row.acc.DeletedAt,
            row.p.AnonymizedAt,
            failedLogins,
            linkedAssisted,
            transports);
    }
}
