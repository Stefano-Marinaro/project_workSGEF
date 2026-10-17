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
public sealed class GetCaregiverInvitePreviewController(GetCaregiverInvitePreviewService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPreview(string token, CancellationToken ct)
    {
        var callerId = Guid.Parse(User.FindFirstValue("sub")!);
        var preview = await service.PreviewAsync(token, callerId, ct);
        return Ok(preview);
    }
}
