using GoCare.Services.Domain;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Domain;

[ApiController]
[Route("addresses")]
[Tags("Indirizzi")]
[Authorize]
public sealed class SearchAddressController(SearchAddressService service) : ControllerBase
{
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q, CancellationToken ct) =>
        Ok(await service.SearchAsync(q, ct));
}
