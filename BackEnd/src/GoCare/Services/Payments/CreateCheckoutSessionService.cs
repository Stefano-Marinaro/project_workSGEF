using GoCare.Data;
using GoCare.Errors;
using GoCare.Models.Domain;
using GoCare.Models.Enums;
using GoCare.Services.Auth;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Stripe;
using Stripe.Checkout;

namespace GoCare.Services.Payments;

public sealed class CreateCheckoutSessionService(
    GoCareDbContext db,
    IStripeClient stripeClient,
    IOptions<PricingOptions> pricingOptions,
    IOptions<FrontendOptions> frontendOptions)
{
    public async Task<string> CreateSessionAsync(Guid callerId, Guid transportRequestId, CancellationToken ct)
    {
        var trip = await db.TransportRequests.SingleOrDefaultAsync(t => t.Id == transportRequestId, ct)
            ?? throw new NotFoundException("Trasporto non trovato.");

        var isLinked = await db.CaregiverAssistedLinks.AnyAsync(
            l => l.CaregiverId == callerId && l.AssistedPersonId == trip.BeneficiaryId && l.DeletedAt == null, ct);
        if (!isLinked)
            throw new ForbiddenException("Non sei collegato a questo assistito.");

        // "confermato" implica già assegnato a un'associazione (TransportRequest.Accept() imposta
        // RequestStatus e AssignedAssociationId insieme), quindi un solo controllo basta.
        if (trip.RequestStatus != ETripRequestStatus.Confirmed)
            throw new ConflictException("Si può pagare solo un trasporto confermato.");

        var pricing = pricingOptions.Value;
        var amount = Math.Max((decimal)trip.DistanceKm * pricing.PricePerKm, pricing.MinimumAmount);

        // Stripe vuole l'importo nell'unità minima della valuta (centesimi per l'euro), non in euro.
        var amountInCents = (long)Math.Round(amount * 100, MidpointRounding.AwayFromZero);

        var paymentId = Guid.NewGuid();

        var sessionOptions = new SessionCreateOptions
        {
            Mode = "payment",
            LineItems =
            [
                new SessionLineItemOptions
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "eur",
                        UnitAmount = amountInCents,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Trasporto GoCare",
                        },
                    },
                },
            ],
            SuccessUrl = frontendOptions.Value.PaymentSuccessUrl,
            CancelUrl = frontendOptions.Value.PaymentCancelUrl,
            Metadata = new Dictionary<string, string> { ["paymentId"] = paymentId.ToString() },
        };

        var sessionService = new SessionService(stripeClient);
        var session = await sessionService.CreateAsync(sessionOptions, cancellationToken: ct);

        var payment = new Payment(paymentId, transportRequestId, amount, "EUR", session.Id, DateTimeOffset.UtcNow);
        db.Payments.Add(payment);
        await db.SaveChangesAsync(ct);

        return session.Url;
    }
}
