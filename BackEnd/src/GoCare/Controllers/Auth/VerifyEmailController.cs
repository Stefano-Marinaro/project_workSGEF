using GoCare.Dtos.Auth.Requests;
using GoCare.Services.Auth;

using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Auth;

[ApiController]
[Route("auth")]
[Tags("Auth")]
public sealed class VerifyEmailController(VerifyEmailService service) : ControllerBase
{
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail(VerifyEmailRequest request, CancellationToken ct)
    {
        await service.VerifyAccount(request.Token, ct);
        return NoContent();
    }
}
