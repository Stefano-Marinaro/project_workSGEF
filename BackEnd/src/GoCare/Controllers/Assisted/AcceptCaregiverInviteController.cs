using System.Security.Claims;

using GoCare.Models.Enums;
using GoCare.Services.Domain;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Assisted;

[ApiController]
[Route("caregiver-invites/{token}")]
[Tags("Assistiti")]
[Authorize(Roles = nameof(EAccountRole.Person))]
public sealed class AcceptCaregiverInviteController(AcceptCaregiverInviteService service) : ControllerBase
{
    [HttpPost("accept")]
    public async Task<IActionResult> Accept(string token, CancellationToken ct)
    {
        var callerId = Guid.Parse(User.FindFirstValue("sub")!);
        await service.AcceptAsync(token, callerId, ct);
        return NoContent();
    }
}
