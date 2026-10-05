using System.Security.Claims;

using GoCare.Models.Enums;
using GoCare.Services.Transport;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.TransportRequest;

[ApiController]
[Route("association/transports")]
[Tags("Trasporti")]
[Authorize(Roles = nameof(EAccountRole.Association))]
public sealed class GenerateOperatorLinkController(GenerateOperatorLinkService service) : ControllerBase
{
    [HttpPost("{id:guid}/operator-link")]
    public async Task<IActionResult> Generate(Guid id, CancellationToken ct)
    {
        var associationId = Guid.Parse(User.FindFirstValue("sub")!);

        var response = await service.GenerateOperatorLinkAsync(id, associationId, ct);

        return Ok(response);
    }
}
