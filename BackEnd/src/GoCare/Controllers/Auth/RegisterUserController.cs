using GoCare.Dtos.Auth.Requests;
using GoCare.Dtos.Auth.Responses;
using GoCare.Services.Auth;

using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Auth;

[ApiController]
[Route("auth")]
[Tags("Auth")]
public sealed class RegisterUserController(RegisterUserService service) : ControllerBase
{
    [HttpPost("register/user")]
    public async Task<ActionResult<RegisterResponse>> RegisterUser(RegisterUserRequest request, CancellationToken ct)
    {
        var accountId = await service.RegisterUserAsync(request.Email, request.Password, ct);

        return Created($"/auth/account/{accountId}", new RegisterResponse(accountId));
    }
}
