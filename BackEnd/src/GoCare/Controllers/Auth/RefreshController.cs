using GoCare.Dtos.Auth.Requests;
using GoCare.Dtos.Auth.Responses;
using GoCare.Services.Auth;

using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Auth;

[ApiController]
[Route("auth")]
[Tags("Auth")]
public sealed class RefreshController(RefreshService service) : ControllerBase
{
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthTokenResponse>> Refresh(RefreshRequest request, CancellationToken ct)
    {
        var (accessToken, refreshToken) = await service.RefreshAsync(request.RefreshToken, ct);

        return Ok(new AuthTokenResponse(accessToken, refreshToken));
    }
}
