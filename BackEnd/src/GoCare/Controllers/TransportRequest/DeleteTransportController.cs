using System.Security.Claims;

using GoCare.Models.Enums;
using GoCare.Services.Transport;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.TransportRequest;


[ApiController]
[Route("transports")]
[Tags("Trasporti")]
[Authorize(Roles = nameof(EAccountRole.Person))]
public sealed class DeleteTransportController(DeleteTransportService delete) : ControllerBase
{
    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> DeleteTransport(Guid id, CancellationToken ct)
    {
        var requestedBy = Guid.Parse(User.FindFirstValue("sub")!);

        await delete.DeleteTransport(id, requestedBy, ct);

        return NoContent();
    }
}
