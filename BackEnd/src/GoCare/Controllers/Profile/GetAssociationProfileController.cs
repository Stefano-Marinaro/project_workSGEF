using System.Security.Claims;

using GoCare.Models.Enums;
using GoCare.Services.Profile;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Profile;

[ApiController]
[Route("association/profile")]
[Tags("Profilo")]
[Authorize(Roles = nameof(EAccountRole.Association))]
public sealed class GetAssociationProfileController(GetAssociationProfileService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var accountId = Guid.Parse(User.FindFirstValue("sub")!);

        var profile = await service.GetAsync(accountId, ct);

        return Ok(profile);
    }
}
