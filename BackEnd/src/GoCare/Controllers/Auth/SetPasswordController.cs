using GoCare.Dtos.Auth.Requests;
using GoCare.Services.Auth;

using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Auth;

[ApiController]
[Route("auth")]
[Tags("Auth")]
public sealed class SetPasswordController(SetPasswordService service) : ControllerBase
{
    // pubblica: l'autorizzazione è il token di invito ricevuto via email
    [HttpPost("set-password")]
    public async Task<IActionResult> SetPassword(SetPasswordRequest request, CancellationToken ct)
    {
        await service.SetPasswordAsync(request.Token, request.Password, ct);
        return NoContent();
    }
}
