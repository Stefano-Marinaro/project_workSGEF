using GoCare.Models.Enums;
using GoCare.Services.Provisioning;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Domain;

[ApiController]
[Authorize(Roles = nameof(EAccountRole.Admin))]
[Route("admin/associations")]
[Tags("Admin")]
public sealed class AccreditAssociationController(
    ProfileProvisioningService provisioning) : ControllerBase
{
    [HttpPost("{associationId:guid}/accredit")]
    public async Task<IActionResult> Accredit(Guid associationId, CancellationToken ct)
    {
        await provisioning.AccreditAssociationAsync(associationId, ct);
        return NoContent();
    }
}
