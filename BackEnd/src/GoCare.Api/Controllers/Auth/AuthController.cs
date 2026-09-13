using GoCare.Api.Dtos.Auth.Requests;
using GoCare.Api.Dtos.Auth.Responses;
using GoCare.Application.Services.Auth;

using Microsoft.AspNetCore.Mvc;

namespace GoCare.Api.Controllers.Auth;

[ApiController]
[Route("auth")]
public sealed class AuthController(IAuthService authservice) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<AuthTokenResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var (accessToken, refreshToken) = await authservice.LoginAsync(request.Email, request.Password, ct);

        return Ok(new AuthTokenResponse(accessToken, refreshToken));
    }
}
