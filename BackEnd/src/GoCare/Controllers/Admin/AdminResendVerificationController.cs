using GoCare.Models.Enums;
using GoCare.Services.Admin;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Admin;

[ApiController]
[Authorize(Roles = nameof(EAccountRole.Admin))]
[Route("admin/caregivers")]
[Tags("Admin")]
public sealed class AdminResendVerificationController(AdminResendVerificationService service) : ControllerBase
{
    [HttpPost("{id:guid}/resend-verification")]
    public async Task<IActionResult> Resend(Guid id, CancellationToken ct)
    {
        await service.ResendAsync(id, ct);
        return NoContent();
    }
}
