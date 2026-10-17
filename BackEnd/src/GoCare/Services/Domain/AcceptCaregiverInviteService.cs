using GoCare.Data;
using GoCare.Errors;
using GoCare.Models.Domain;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Domain;

public sealed class AcceptCaregiverInviteService(GoCareDbContext db)
{
    public async Task AcceptAsync(string token, Guid callerId, CancellationToken ct)
    {
        var link = await ResolveLinkAsync(token, ct);

        var existingLink = await db.CaregiverAssistedLinks.SingleOrDefaultAsync(
            l => l.CaregiverId == callerId && l.AssistedPersonId == link.AssistedPersonId, ct);

        if (existingLink is not null && existingLink.DeletedAt is null)
            throw new ConflictException("Sei già collegato a questo assistito.");

        if (existingLink is null)
            db.CaregiverAssistedLinks.Add(new CaregiverAssistedLink(callerId, link.AssistedPersonId, DateTimeOffset.UtcNow));
        else
            existingLink.Restore();

        await db.SaveChangesAsync(ct);
    }

    private async Task<CaregiverInviteLink> ResolveLinkAsync(string token, CancellationToken ct)
    {
        var link = await db.CaregiverInviteLinks.SingleOrDefaultAsync(l => l.Token == token, ct)
            ?? throw new NotFoundException("Link non valido.");

        if (!link.IsUsable(DateTimeOffset.UtcNow))
            throw new ForbiddenException("Il link è scaduto o è stato revocato.");

        return link;
    }
}
