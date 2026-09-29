using GoCare.Dtos.Auth.Requests;
using GoCare.Services.Auth;

using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Auth;

[ApiController]
[Route("auth")]
[Tags("Auth")]
public sealed class ForgotPasswordController(ForgotPasswordService service) : ControllerBase
{
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request, CancellationToken ct)
    {
        await service.ForgotPasswordAsync(request.Email, ct);
        return NoContent();
    }
}
