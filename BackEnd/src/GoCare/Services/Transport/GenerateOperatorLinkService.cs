using GoCare.Data;
using GoCare.Dtos.Transport.Responses;
using GoCare.Errors;
using GoCare.Models.Domain;
using GoCare.Models.Enums;
using GoCare.Services.Auth;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GoCare.Services.Transport;

public sealed class GenerateOperatorLinkService(
    GoCareDbContext db,
    TokenService tokenService,
    IOptions<FrontendOptions> frontendOptions)
{
    private readonly FrontendOptions _frontend = frontendOptions.Value;

    public async Task<OperatorLinkResponse> GenerateOperatorLinkAsync(Guid transportId, Guid associationId, CancellationToken ct)
    {
        var transport = await db.TransportRequests.SingleOrDefaultAsync(t => t.Id == transportId, ct)
            ?? throw new NotFoundException("Trasporto non trovato.");

        if (transport.AssignedAssociationId != associationId)
            throw new ForbiddenException("Il trasporto non è assegnato alla tua associazione.");

        if (transport.RequestStatus is not (ETripRequestStatus.Confirmed or ETripRequestStatus.InProgress))
            throw new ConflictException("Il link operatore è generabile solo per trasporti confermati o in corso.");

        // Il volontario si assegna solo dopo il pagamento confermato dal webhook Stripe
        if (!await IsPaidAsync(transportId, ct))
            throw new ConflictException("Il volontario può essere assegnato solo dopo il pagamento del caregiver.");

        var now = DateTimeOffset.UtcNow;

        var link = new TripOperatorLink(
            Guid.NewGuid(),
            transportId,
            tokenService.GenerateRefreshTokenValue(),
            expiresAt: transport.DepartureDateHour.AddHours(24),
            createdAt: now);

        db.TripOperatorLinks.Add(link);
        await db.SaveChangesAsync(ct);

        var url = $"{_frontend.OperatorUrl}/{link.Token}";

        return new OperatorLinkResponse(url, link.ExpiresAt);
    }

    // Pagato = esiste un pagamento Succeeded (lo stato lo imposta solo il webhook checkout.session.completed)
    private Task<bool> IsPaidAsync(Guid transportId, CancellationToken ct) =>
        db.Payments.AnyAsync(p => p.TransportRequestId == transportId && p.Status == EPaymentStatus.Succeeded, ct);
}
