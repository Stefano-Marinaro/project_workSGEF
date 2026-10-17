using System.Security.Claims;

using GoCare.Models.Enums;
using GoCare.Services.Domain;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Assisted;

[ApiController]
[Route("assisted")]
[Tags("Assistiti")]
[Authorize(Roles = nameof(EAccountRole.Person))]
public sealed class GenerateCaregiverInviteLinkController(GenerateCaregiverInviteLinkService service) : ControllerBase
{
    [HttpPost("{id:guid}/caregiver-invite-link")]
    public async Task<IActionResult> Generate(Guid id, CancellationToken ct)
    {
        var callerId = Guid.Parse(User.FindFirstValue("sub")!);
        var response = await service.GenerateAsync(callerId, id, ct);
        return Ok(response);
    }
}
