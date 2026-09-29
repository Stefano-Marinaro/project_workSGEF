using System.Security.Claims;

using GoCare.Dtos.Auth.Requests;
using GoCare.Errors;
using GoCare.Services.Auth;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Auth;

[ApiController]
[Route("auth")]
[Tags("Auth")]
public sealed class ChangeEmailController(ChangeEmailService service) : ControllerBase
{
    [Authorize]
    [HttpPost("change-email")]
    public async Task<IActionResult> ChangeEmail(ChangeEmailRequest request, CancellationToken ct)
    {
        var subject = User.FindFirstValue("sub");

        if (!Guid.TryParse(subject, out var accountId))
            throw new ForbiddenException("Il token non contiene un identificativo account valido.");

        await service.RequestEmailChangeAsync(
            accountId,
            request.NewEmail,
            request.CurrentPassword,
            ct);

        return NoContent();
    }
}
