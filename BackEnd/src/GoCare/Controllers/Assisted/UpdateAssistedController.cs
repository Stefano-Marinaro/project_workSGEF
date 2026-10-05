using System.Security.Claims;

using GoCare.Dtos.Domain.Requests;
using GoCare.Dtos.Transport.Requests;
using GoCare.Models.Enums;
using GoCare.Services.Domain;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Assisted;

[ApiController]
[Route("assisted")]
[Tags("Assistiti")]
[Authorize(Roles = nameof(EAccountRole.Person))]
public sealed class UpdateAssistedController(AssistedPersonService assistedPeople) : ControllerBase
{
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, AssistedPersonRequest request, CancellationToken ct)
    {
        var caregiverId = Guid.Parse(User.FindFirstValue("sub")!);

        await assistedPeople.UpdateAsync(
            caregiverId,
            id,
            request.Name,
            request.Surname,
            request.BirthDate,
            request.Phone,
            request.HomeAddress?.ToAddress(),
            ct);

        return NoContent();
    }
}
