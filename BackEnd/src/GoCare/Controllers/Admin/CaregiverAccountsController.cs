using GoCare.Models.Enums;
using GoCare.Pagination;
using GoCare.Services.Admin;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Admin;

[ApiController]
[Authorize(Roles = nameof(EAccountRole.Admin))]
[Route("admin/caregivers")]
[Tags("Admin")]
public sealed class CaregiverAccountsController(
    ListCaregiverAccountsService listService,
    CaregiverAccountDetailService detailService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? search,
        [FromQuery] EAccountStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default) =>
        Ok(await listService.ListAsync(search, status, new PageQuery(page, pageSize), ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken ct) =>
        Ok(await detailService.GetAsync(id, ct));
}
