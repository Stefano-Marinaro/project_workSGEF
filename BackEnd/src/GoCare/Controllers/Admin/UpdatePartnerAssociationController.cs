using GoCare.Dtos.Admin.Requests;
using GoCare.Dtos.Transport.Requests;
using GoCare.Models.Enums;
using GoCare.Services.Admin;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Admin;

[ApiController]
[Authorize(Roles = nameof(EAccountRole.Admin))]
[Route("admin/associations")]
[Tags("Admin")]
public sealed class UpdatePartnerAssociationController(UpdatePartnerAssociationService service) : ControllerBase
{
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdatePartnerAssociationRequest request, CancellationToken ct)
    {
        var emailConfirmationSent = await service.UpdateAsync(
            id,
            request.Email,
            request.Name,
            request.Headquarter.ToAddress(),
            request.Phones,
            request.CoveredComuni,
            request.AvailabilityHours,
            ct);

        // 202: modifiche salvate, ma l'email cambierà solo dopo la conferma al nuovo indirizzo
        return emailConfirmationSent ? Accepted() : NoContent();
    }
}
