using System.Security.Claims;

using GoCare.Dtos.Transport.Responses;
using GoCare.Models.Enums;
using GoCare.Services.Payments;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.TransportRequest;

[ApiController]
[Route("transports")]
[Tags("Trasporti")]
[Authorize(Roles = nameof(EAccountRole.Person))]
public sealed class CreateCheckoutSessionController(CreateCheckoutSessionService checkout) : ControllerBase
{
    [HttpPost("{id:guid}/payment-session")]
    public async Task<IActionResult> CreateCheckoutSession(Guid id, CancellationToken ct)
    {
        var callerId = Guid.Parse(User.FindFirstValue("sub")!);

        var paymentUrl = await checkout.CreateSessionAsync(callerId, id, ct);

        return Ok(new CreateCheckoutSessionResponse(paymentUrl));
    }
}
