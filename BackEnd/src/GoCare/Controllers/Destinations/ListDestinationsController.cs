using System.Security.Claims;

using GoCare.Models.Enums;
using GoCare.Services.Destinations;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Destinations;

[ApiController]
[Route("me/destinations")]
[Tags("Destinazioni")]
[Authorize(Roles = nameof(EAccountRole.Person))]
public sealed class ListDestinationsController(ListDestinationsService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var personId = Guid.Parse(User.FindFirstValue("sub")!);

        var destinations = await service.ListAsync(personId, ct);

        return Ok(destinations);
    }
}
