using GoCare.Models.Enums;
using GoCare.Services.Admin;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Admin;

[ApiController]
[Authorize(Roles = nameof(EAccountRole.Admin))]
[Route("admin/associations")]
[Tags("Admin")]
public sealed class PartnerAssociationDetailController(PartnerAssociationDetailService service) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken ct) =>
        Ok(await service.GetAsync(id, ct));
}
