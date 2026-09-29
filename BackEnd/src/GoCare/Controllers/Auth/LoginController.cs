using GoCare.Dtos.Auth.Requests;
using GoCare.Dtos.Auth.Responses;
using GoCare.Services.Auth;

using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Auth;

[ApiController]
[Route("auth")]
[Tags("Auth")]
public sealed class LoginController(LoginService service) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<AuthTokenResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var (accessToken, refreshToken) = await service.LoginAsync(request.Email, request.Password, ct);

        return Ok(new AuthTokenResponse(accessToken, refreshToken));
    }
}
