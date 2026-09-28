using GoCare.Models.Enums;
using GoCare.Services.Provisioning;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Domain;

[ApiController]
[Authorize(Roles = nameof(EAccountRole.Admin))]
[Route("admin/associations")]
[Tags("Admin")]
public sealed class RejectAssociationController(
    ProfileProvisioningService provisioning) : ControllerBase
{
    [HttpPost("{associationId:guid}/reject")]
    public async Task<IActionResult> Reject(Guid associationId, CancellationToken ct)
    {
        await provisioning.RejectAssociationAsync(associationId, ct);
        return NoContent();
    }
}
