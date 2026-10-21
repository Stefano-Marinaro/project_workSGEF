using System.Security.Claims;

using GoCare.Models.Enums;
using GoCare.Services.Transport;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.TransportRequest;

[ApiController]
[Route("association/requests")]
[Tags("Trasporti")]
[Authorize(Roles = nameof(EAccountRole.Association))]
public sealed class PendingRequestDetailController(PendingRequestDetailService service) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken ct)
    {
        var associationId = Guid.Parse(User.FindFirstValue("sub")!);

        var request = await service.GetAsync(associationId, id, ct);

        return Ok(request);
    }
}
