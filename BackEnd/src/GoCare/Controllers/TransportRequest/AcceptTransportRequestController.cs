using System.Security.Claims;

using GoCare.Models.Enums;
using GoCare.Services.Transport;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.TransportRequest;

[ApiController]
[Route("association/requests")]
[Tags("Trasporti")]
[Authorize(Roles = nameof(EAccountRole.Association))]
public sealed class AcceptTransportRequestController(AcceptTransportRequestService service) : ControllerBase
{
    [HttpPost("{id:guid}/accept")]
    public async Task<IActionResult> Accept(Guid id, CancellationToken ct)
    {
        var associationId = Guid.Parse(User.FindFirstValue("sub")!);

        await service.AcceptAsync(id, associationId, ct);

        return NoContent();
    }
}
