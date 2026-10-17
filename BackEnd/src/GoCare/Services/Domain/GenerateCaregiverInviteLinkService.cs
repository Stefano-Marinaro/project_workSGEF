using GoCare.Data;
using GoCare.Dtos.Domain.Responses;
using GoCare.Errors;
using GoCare.Models.Domain;
using GoCare.Services.Auth;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GoCare.Services.Domain;

public sealed class GenerateCaregiverInviteLinkService(
    GoCareDbContext db,
    TokenService tokenService,
    IOptions<FrontendOptions> frontendOptions)
{
    private readonly FrontendOptions _frontend = frontendOptions.Value;

    public async Task<CaregiverInviteLinkResponse> GenerateAsync(Guid callerId, Guid assistedPersonId, CancellationToken ct)
    {
        var isLinked = await db.CaregiverAssistedLinks.AnyAsync(
            l => l.CaregiverId == callerId && l.AssistedPersonId == assistedPersonId && l.DeletedAt == null, ct);

        if (!isLinked)
            throw new ForbiddenException("Non sei collegato a questo assistito.");

        var now = DateTimeOffset.UtcNow;

        var link = new CaregiverInviteLink(
            Guid.NewGuid(), assistedPersonId, callerId, tokenService.GenerateRefreshTokenValue(),
            expiresAt: now.AddDays(7), createdAt: now);

        db.CaregiverInviteLinks.Add(link);
        await db.SaveChangesAsync(ct);

        var url = $"{_frontend.CaregiverInviteUrl}/{link.Token}";
        return new CaregiverInviteLinkResponse(url, link.ExpiresAt);
    }
}
