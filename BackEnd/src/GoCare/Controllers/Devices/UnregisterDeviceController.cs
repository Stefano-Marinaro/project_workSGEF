using System.Security.Claims;

using GoCare.Models.Enums;
using GoCare.Services.Devices;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Devices;

[ApiController]
[Route("devices")]
[Tags("Dispositivi")]
[Authorize(Roles = $"{nameof(EAccountRole.Person)},{nameof(EAccountRole.Association)}")]
public sealed class UnregisterDeviceController(UnregisterDeviceService service) : ControllerBase
{
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Unregister(Guid id, CancellationToken ct)
    {
        var subjectType = User.FindFirstValue("role") == nameof(EAccountRole.Association)
            ? ENotificationSubject.Association
            : ENotificationSubject.Person;
        var subjectId = Guid.Parse(User.FindFirstValue("sub")!);

        await service.UnregisterAsync(subjectType, subjectId, id, ct);

        return NoContent();
    }
}
