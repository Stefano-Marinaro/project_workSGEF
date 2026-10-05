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
public sealed class GetAssistedController(AssistedPersonService assistedPeople) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken ct)
    {
        var caregiverId = Guid.Parse(User.FindFirstValue("sub")!);

        var assisted = await assistedPeople.GetDetailAsync(caregiverId, id, ct);

        return Ok(assisted);
    }
}
