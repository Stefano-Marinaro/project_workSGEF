using GoCare.Services.Transport;

using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.TransportRequest;

[ApiController]
[Route("operator/{token}")]
[Tags("Operatore trasporto")]
public sealed class OperatorTripStatusController(GetTripExecutionStatusService service) : ControllerBase
{
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus(string token, CancellationToken ct)
    {
        var status = await service.GetStatusAsync(token, ct);
        return Ok(status);
    }
}
