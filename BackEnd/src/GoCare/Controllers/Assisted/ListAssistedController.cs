using System.Security.Claims;

using GoCare.Models.Enums;
using GoCare.Services.Domain;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Assisted;

[ApiController]
[Route("assisted")]
[Tags("Assistiti")]
[Authorize(Roles = nameof(EAccountRole.Person))]
public sealed class ListAssistedController(AssistedPersonService assistedPeople) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var caregiverId = Guid.Parse(User.FindFirstValue("sub")!);

        var assisted = await assistedPeople.ListAsync(caregiverId, ct);

        return Ok(assisted);
    }
}
