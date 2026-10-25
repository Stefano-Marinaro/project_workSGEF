using System.Security.Claims;

using GoCare.Dtos.Domain.Requests;
using GoCare.Dtos.Transport.Requests;
using GoCare.Models.Enums;
using GoCare.Services.Profile;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Profile;

[ApiController]
[Route("association/profile")]
[Tags("Profilo")]
[Authorize(Roles = nameof(EAccountRole.Association))]
public sealed class UpdateAssociationProfileController(UpdateAssociationProfileService service) : ControllerBase
{
    [HttpPut]
    public async Task<IActionResult> Update(UpdateAssociationProfileRequest request, CancellationToken ct)
    {
        var accountId = Guid.Parse(User.FindFirstValue("sub")!);

        await service.UpdateAsync(
            accountId,
            request.Name,
            request.Headquarter.ToAddress(),
            request.Phones,
            request.CoveredComuni,
            request.AvailabilityHours,
            ct);

        return NoContent();
    }
}