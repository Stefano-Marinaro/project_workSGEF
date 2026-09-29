using GoCare.Dtos.Auth.Requests;
using GoCare.Services.Auth;

using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Auth;

[ApiController]
[Route("auth")]
[Tags("Auth")]
public sealed class ConfirmEmailChangeController(ConfirmEmailChangeService service) : ControllerBase
{
    [HttpPost("change-email/confirm")]
    public async Task<IActionResult> ConfirmEmailChange(
        ConfirmEmailChangeRequest request,
        CancellationToken ct)
    {
        await service.ConfirmEmailChangeAsync(request.Token, ct);
        return NoContent();
    }
}
