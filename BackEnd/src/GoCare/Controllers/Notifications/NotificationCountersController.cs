using System.Security.Claims;

using GoCare.Models.Enums;
using GoCare.Services.Notifications;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Notifications;

[ApiController]
[Route("notifications")]
[Tags("Notifiche")]
[Authorize(Roles = $"{nameof(EAccountRole.Person)},{nameof(EAccountRole.Association)}")]
public sealed class NotificationCountersController(NotificationCountersService service) : ControllerBase
{
    [HttpGet("counters")]
    public async Task<IActionResult> GetCounters(CancellationToken ct)
    {
        var subjectType = User.FindFirstValue("role") == nameof(EAccountRole.Association)
            ? ENotificationSubject.Association
            : ENotificationSubject.Person;
        var subjectId = Guid.Parse(User.FindFirstValue("sub")!);

        var counters = await service.GetCountersAsync(subjectType, subjectId, ct);

        return Ok(counters);
    }
}
