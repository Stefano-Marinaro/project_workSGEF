using System.Security.Claims;

using GoCare.Models.Enums;
using GoCare.Services.Transport;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.TransportRequest;

[ApiController]
[Route("association/transports")]
[Tags("Trasporti")]
[Authorize(Roles = nameof(EAccountRole.Association))]
public sealed class ListAcceptedTransportsController(ListAcceptedTransportsService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var associationId = Guid.Parse(User.FindFirstValue("sub")!);

        var trips = await service.ListAsync(associationId, ct);

        return Ok(trips);
    }
}
