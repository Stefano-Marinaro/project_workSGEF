using System.Security.Claims;

using GoCare.Dtos.Transport.Requests;
using GoCare.Models.Enums;
using GoCare.Services.Transport;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.TransportRequest;

[ApiController]
[Route("association/requests")]
[Tags("Trasporti")]
[Authorize(Roles = nameof(EAccountRole.Association))]
public sealed class DeclineTransportRequestController(DeclineTransportRequestService service) : ControllerBase
{
    [HttpPost("{id:guid}/decline")]
    public async Task<IActionResult> Decline(Guid id, DeclineTransportRequestRequest request, CancellationToken ct)
    {
        var associationId = Guid.Parse(User.FindFirstValue("sub")!);

        await service.DeclineAsync(id, associationId, request.Reason, ct);

        return NoContent();
    }
}
