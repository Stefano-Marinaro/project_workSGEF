using System.Security.Claims;

using GoCare.Dtos.Devices.Requests;
using GoCare.Dtos.Devices.Responses;
using GoCare.Models.Enums;
using GoCare.Services.Devices;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Devices;

[ApiController]
[Route("devices")]
[Tags("Dispositivi")]
[Authorize(Roles = $"{nameof(EAccountRole.Person)},{nameof(EAccountRole.Association)}")]
public sealed class RegisterDeviceController(RegisterDeviceService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Register(RegisterDeviceRequest request, CancellationToken ct)
    {
        var subjectType = User.FindFirstValue("role") == nameof(EAccountRole.Association)
            ? ENotificationSubject.Association
            : ENotificationSubject.Person;
        var subjectId = Guid.Parse(User.FindFirstValue("sub")!);

        var id = await service.RegisterAsync(subjectType, subjectId, request.PushToken, request.Platform, ct);

        return Created($"/devices/{id}", new DeviceResponse(id));
    }
}
