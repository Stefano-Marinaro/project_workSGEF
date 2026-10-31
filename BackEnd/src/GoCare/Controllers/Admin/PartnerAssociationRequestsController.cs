using GoCare.Models.Enums;
using GoCare.Pagination;
using GoCare.Services.Admin;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Admin;

[ApiController]
[Authorize(Roles = nameof(EAccountRole.Admin))]
[Route("admin/associations")]
[Tags("Admin")]
public sealed class PartnerAssociationRequestsController(PartnerAssociationRequestsService service) : ControllerBase
{
    [HttpGet("{id:guid}/requests")]
    public async Task<IActionResult> List(
        Guid id,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] ECandidateOutcome? outcome,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default) =>
        Ok(await service.ListAsync(id, from, to, outcome, new PageQuery(page, pageSize), ct));
}
