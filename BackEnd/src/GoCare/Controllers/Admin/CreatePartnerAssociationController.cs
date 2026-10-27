using GoCare.Dtos.Admin.Requests;
using GoCare.Dtos.Auth.Responses;
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
public sealed class CreatePartnerAssociationController(CreatePartnerAssociationService service) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<RegisterResponse>> Create(CreatePartnerAssociationRequest request, CancellationToken ct)
    {
        var associationId = await service.CreateAsync(
            request.Email,
            request.Name,
            request.Headquarter.ToAddress(),
            request.Phones,
            request.CoveredComuni,
            request.AvailabilityHours,
            ct);

        return Created($"/admin/associations/{associationId}", new RegisterResponse(associationId));
    }
}
