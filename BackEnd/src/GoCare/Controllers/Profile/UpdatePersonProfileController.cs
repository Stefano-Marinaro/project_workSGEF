using System.Security.Claims;

using GoCare.Dtos.Domain.Requests;
using GoCare.Dtos.Transport.Requests;
using GoCare.Models.Enums;
using GoCare.Services.Profile;

using Microsoft.AspNetCore.Authorization;

// importa gli strumenti per costruire i controller
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Profile;

[ApiController]
[Route("me/profile")]
// è un'etichetta, serve a raggruppare gli endpoint sotto Profilo nella pagina Swagger
[Tags("Profilo")]
[Authorize(Roles = nameof(EAccountRole.Person))]
public sealed class UpdatePersonProfileController(UpdatePersonProfileService service) : ControllerBase
{
    [HttpPut]
    public async Task<IActionResult> Update(UpdatePersonProfileRequest request, CancellationToken ct)
    {
        var accountId = Guid.Parse(User.FindFirstValue("sub")!);


        await service.UpdateAsync(
            accountId,
            request.Name,
            request.Surname,
            request.BirthDate,
            request.Phone,
            request.HomeAddress?.ToAddress(),
            ct
        );

        return NoContent();
    }
}