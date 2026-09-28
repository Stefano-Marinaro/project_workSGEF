using System.Security.Claims;

using GoCare.Dtos.Transport.Requests;
using GoCare.Services.Transport;

using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.TransportRequest;

[ApiController]
[Route("transports")]
[Tags("Trasporti")]
public sealed class DeleteTransportController(DeleteTransportService delete) : ControllerBase
{
    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> DeleteTranasport(Guid id, CancellationToken ct)
    {
        var requestedBy = Guid.Parse(User.FindFirstValue("sub")!);

        await delete.DeleteTransport(id, requestedBy, ct);

        return NoContent();
    }
}
