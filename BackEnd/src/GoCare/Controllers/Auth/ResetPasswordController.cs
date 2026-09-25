using GoCare.Dtos.Auth.Requests;
using GoCare.Services.Auth;

using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Auth;

[ApiController]
[Route("auth")]
[Tags("Auth")]
public sealed class ResetPasswordController(ResetPasswordService service) : ControllerBase
{
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request, CancellationToken ct)
    {
        await service.ResetPasswordAsync(request.Token, request.NewPassword, ct);
        return NoContent();
    }
}
