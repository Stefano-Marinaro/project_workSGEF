using GoCare.Models.Enums;
using GoCare.Services.Provisioning;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Domain.Admin;

[ApiController]
[Authorize(Roles = nameof(EAccountRole.Admin))]
[Route("admin/associations")]
public sealed class AssociationAccreditationController(
    ProfileProvisioningService provisioning) : ControllerBase
{
    [HttpPost("{associationId:guid}/accredit")]
    public async Task<IActionResult> Accredit(Guid associationId, CancellationToken ct)
    {
        await provisioning.AccreditAssociationAsync(associationId, ct);
        return NoContent();
    }

    [HttpPost("{associationId:guid}/reject")]
    public async Task<IActionResult> Reject(Guid associationId, CancellationToken ct)
    {
        await provisioning.RejectAssociationAsync(associationId, ct);
        return NoContent();
    }
}
