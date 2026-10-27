using GoCare.Models.Enums;
using GoCare.Services.Admin;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Admin;

[ApiController]
[Authorize(Roles = nameof(EAccountRole.Admin))]
[Route("admin/caregivers")]
[Tags("Admin")]
public sealed class AdminPasswordResetController(AdminPasswordResetService service) : ControllerBase
{
    [HttpPost("{id:guid}/password-reset")]
    public async Task<IActionResult> Reset(Guid id, CancellationToken ct)
    {
        await service.ResetAsync(id, ct);
        return NoContent();
    }
}
