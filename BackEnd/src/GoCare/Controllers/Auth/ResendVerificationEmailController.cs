using GoCare.Dtos.Auth.Requests;
using GoCare.Services.Auth;

using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Auth;

[ApiController]
[Route("auth")]
[Tags("Auth")]
public sealed class ResendVerificationEmailController(ResendVerificationEmailService service) : ControllerBase
{
    [HttpPost("verify-email/resend")]
    public async Task<IActionResult> ResendVerificationEmail(
        ResendVerificationEmailRequest request,
        CancellationToken ct)
    {
        await service.ResendVerificationEmailAsync(request.Email, ct);
        return NoContent();
    }
}
