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
public sealed class MarkNotificationReadController(MarkNotificationReadService service) : ControllerBase
{
    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct)
    {
        var subjectType = User.FindFirstValue("role") == nameof(EAccountRole.Association)
            ? ENotificationSubject.Association
            : ENotificationSubject.Person;
        var subjectId = Guid.Parse(User.FindFirstValue("sub")!);

        await service.MarkReadAsync(subjectType, subjectId, id, ct);

        return NoContent();
    }
}
