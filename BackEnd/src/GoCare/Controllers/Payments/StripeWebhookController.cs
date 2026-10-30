using GoCare.Services.Payments;

using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Payments;

// Nessun [Authorize]: a chiamare questo endpoint è Stripe stesso, non un utente loggato.
// L'unico controllo di sicurezza è la verifica della firma, dentro StripeWebhookService.
[ApiController]
[Route("payments/stripe")]
[Tags("Pagamenti")]
public sealed class StripeWebhookController(StripeWebhookService webhook) : ControllerBase
{
    [HttpPost("webhook")]
    public async Task<IActionResult> HandleWebhook(CancellationToken ct)
    {
        using var reader = new StreamReader(Request.Body);
        var requestBody = await reader.ReadToEndAsync(ct);
        var signatureHeader = Request.Headers["Stripe-Signature"].ToString();

        await webhook.HandleAsync(requestBody, signatureHeader, ct);

        return Ok();
    }
}
