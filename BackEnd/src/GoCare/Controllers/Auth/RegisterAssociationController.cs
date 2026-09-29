using GoCare.Dtos.Auth.Requests;
using GoCare.Dtos.Auth.Responses;
using GoCare.Services.Auth;

using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Auth;

[ApiController]
[Route("auth")]
[Tags("Auth")]
public sealed class RegisterAssociationController(RegisterAssociationService service) : ControllerBase
{
    [HttpPost("register/association")]
    public async Task<ActionResult<RegisterResponse>> RegisterAssociation(RegisterAssociationRequest request, CancellationToken ct)
    {
        var accountId = await service.RegisterAssociationAsync(request.Email, request.Password, ct);

        return Created($"/auth/account/{accountId}", new RegisterResponse(accountId));
    }
}
