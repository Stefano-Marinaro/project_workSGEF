using System.Security.Claims;

using GoCare.Models.Enums;
using GoCare.Services.Admin;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Admin;

[ApiController]
[Authorize(Roles = nameof(EAccountRole.Admin))]
[Route("admin/caregivers")]
[Tags("Admin")]
public sealed class RestoreCaregiverAccountController(RestoreCaregiverAccountService service) : ControllerBase
{
    [HttpPost("{id:guid}/restore")]
    public async Task<IActionResult> Restore(Guid id, CancellationToken ct)
    {
        var adminId = Guid.Parse(User.FindFirstValue("sub")!);

        await service.RestoreAsync(id, adminId, ct);

        return NoContent();
    }
}
