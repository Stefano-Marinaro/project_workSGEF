using System.Security.Claims;

using GoCare.Dtos.Transport.Requests;
using GoCare.Models.Enums;
using GoCare.Services.Transport;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GoCare.Controllers.TransportRequest;

[ApiController]
[Route("association/requests")]
[Tags("Trasporti")]
[Authorize(Roles = nameof(EAccountRole.Association))]
public sealed class DeclineTransportRequestController(DeclineTransportRequestService service) : ControllerBase
{
    // il body è facoltativo: senza body il rifiuto è senza motivo
    [HttpPost("{id:guid}/decline")]
    public async Task<IActionResult> Decline(
        Guid id,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] DeclineTransportRequest? request,
        CancellationToken ct)
    {
        var associationId = Guid.Parse(User.FindFirstValue("sub")!);

        await service.DeclineAsync(id, associationId, request?.Reason, ct);

        return NoContent();
    }
}
