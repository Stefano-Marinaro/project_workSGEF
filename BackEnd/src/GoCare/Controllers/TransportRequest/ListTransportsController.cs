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
public sealed class ListTransportsController(ListTransportsService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListMine([FromQuery] Guid? assistedId, CancellationToken ct)
    {
        var callerId = Guid.Parse(User.FindFirstValue("sub")!);

        var trips = await service.ListMineAsync(callerId, assistedId, ct);

        return Ok(trips);
    }
}
