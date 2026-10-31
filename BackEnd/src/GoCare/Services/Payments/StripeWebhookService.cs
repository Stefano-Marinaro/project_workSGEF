using GoCare.Data;
using GoCare.Errors;
using GoCare.Models.Enums;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Stripe;
using Stripe.Checkout;

namespace GoCare.Services.Payments;

public sealed class StripeWebhookService(GoCareDbContext db, IOptions<StripeOptions> stripeOptions)
{
    // riceve il corpo grezzo della richiesta + l'header della firma, così come arrivano da ASP.NET,
    // senza che nulla li abbia già deserializzati (altrimenti la verifica della firma fallirebbe).
    public async Task HandleAsync(string requestBody, string stripeSignatureHeader, CancellationToken ct)
    {
        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(
                requestBody, stripeSignatureHeader, stripeOptions.Value.WebhookSecret);
        }
        catch (StripeException)
        {
            throw new ForbiddenException("Firma del webhook non valida.");
        }

        if (stripeEvent.Type != "checkout.session.completed")
            return; // altri eventi (es. scadenza sessione) non ci interessano per ora

        var session = stripeEvent.Data.Object as Session
            ?? throw new InvalidOperationException("Evento Stripe senza una Checkout Session valida.");

        var payment = await db.Payments.SingleOrDefaultAsync(p => p.StripeSessionId == session.Id, ct);
        if (payment is null)
            return; // sessione che non conosciamo (es. di un altro ambiente): ignorata, non un errore

        // Idempotenza: Stripe può rimandare lo stesso evento più volte, MarkSucceeded() di un
        // pagamento già Succeeded lancerebbe - qui semplicemente non rifacciamo il lavoro.
        if (payment.Status != EPaymentStatus.Pending)
            return;

        payment.MarkSucceeded(session.PaymentIntentId, DateTimeOffset.UtcNow);
        await db.SaveChangesAsync(ct);
    }
}
