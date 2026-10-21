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
public sealed class ListPartnerAssociationsController(ListPartnerAssociationsService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(
        // [FromQuery] è un attributo che dice a ASP.Net Core da dove prendere il valore di un parametro dal metodo del controller
        [FromQuery] string? search,
        [FromQuery] EAccountStatus? status,
        [FromQuery] string? province,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default) =>
        Ok(await service.ListAsync(search, status, province, new PageQuery(page, pageSize), ct));
}