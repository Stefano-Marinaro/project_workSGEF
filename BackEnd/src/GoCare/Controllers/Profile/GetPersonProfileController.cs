using System.Security.Claims;

using GoCare.Models.Enums;
using GoCare.Services.Profile;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Profile;

[ApiController]
[Route("me/profile")]
[Tags("Profilo")]
[Authorize(Roles = nameof(EAccountRole.Person))]
public sealed class GetPersonProfileController(GetPersonProfileService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var accountId = Guid.Parse(User.FindFirstValue("sub")!);

        var profile = await service.GetAsync(accountId, ct);

        return Ok(profile);
    }
}
