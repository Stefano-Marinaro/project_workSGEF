using GoCare.Data;
using GoCare.Dtos.Domain.Responses;
using GoCare.Errors;
using GoCare.Models.Domain;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Domain;

public sealed class GetCaregiverInvitePreviewService(GoCareDbContext db)
{
    public async Task<CaregiverInvitePreviewResponse> PreviewAsync(string token, Guid callerId, CancellationToken ct)
    {
        var link = await ResolveLinkAsync(token, ct);

        var assistedPerson = await db.AssistedPeople.AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == link.AssistedPersonId && p.DeletedAt == null, ct)
            ?? throw new NotFoundException("Assistito non trovato.");

        var invitedBy = await db.Persons.AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == link.InvitedByPersonId && p.DeletedAt == null, ct)
            ?? throw new NotFoundException("Account non trovato.");

        var alreadyLinked = await db.CaregiverAssistedLinks.AnyAsync(
            l => l.CaregiverId == callerId && l.AssistedPersonId == link.AssistedPersonId && l.DeletedAt == null, ct);

        return new CaregiverInvitePreviewResponse(
            assistedPerson.Name, assistedPerson.Surname,
            invitedBy.Name, invitedBy.Surname,
            alreadyLinked);
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
