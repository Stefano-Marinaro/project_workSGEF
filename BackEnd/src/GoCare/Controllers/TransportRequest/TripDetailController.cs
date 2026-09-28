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
public sealed class TripDetailController(TripDetailService service) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken ct)
    {
        var callerId = Guid.Parse(User.FindFirstValue("sub")!);

        var response = await service.TripDetail(id, callerId, ct);

        return Ok(response);
    }
}
