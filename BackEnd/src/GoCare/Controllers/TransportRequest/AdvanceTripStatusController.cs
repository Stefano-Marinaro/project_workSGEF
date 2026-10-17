using GoCare.Dtos.Transport.Requests;
using GoCare.Services.Transport;

using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.TransportRequest;

[ApiController]
[Route("operator/{token}")]
[Tags("Operatore trasporto")]
public sealed class AdvanceTripStatusController(AdvanceTripStatusService service) : ControllerBase
{
    [HttpPost("status")]
    public async Task<IActionResult> Advance(string token, AdvanceTripStatusRequest request, CancellationToken ct)
    {
        var status = await service.AdvanceAsync(token, request.Action, request.OperatorLabel, ct);
        return Ok(status);
    }
}
