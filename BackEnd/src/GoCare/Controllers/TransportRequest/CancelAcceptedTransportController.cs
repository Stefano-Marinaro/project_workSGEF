using System.Security.Claims;

using GoCare.Dtos.Transport.Requests;
using GoCare.Models.Enums;
using GoCare.Services.Transport;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.TransportRequest;

[ApiController]
[Route("association/transports")]
[Tags("Trasporti")]
[Authorize(Roles = nameof(EAccountRole.Association))]
public sealed class CancelAcceptedTransportController(CancelAcceptedTransportService service) : ControllerBase
{
    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancelAcceptedTransportRequest request, CancellationToken ct)
    {
        var associationId = Guid.Parse(User.FindFirstValue("sub")!);

        await service.CancelAsync(id, associationId, request.Reason, ct);

        return NoContent();
    }
}
