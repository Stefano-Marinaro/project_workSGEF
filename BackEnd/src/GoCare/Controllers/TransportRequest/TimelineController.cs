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
public sealed class TimelineController(TimelineService service) : ControllerBase
{
    [HttpGet("{id:guid}/status-timeline")]
    public async Task<IActionResult> GetTimeline(Guid id, CancellationToken ct)
    {
        var callerId = Guid.Parse(User.FindFirstValue("sub")!);

        var timeline = await service.GetTimelineAsync(id, callerId, ct);

        return Ok(timeline);
    }
}
