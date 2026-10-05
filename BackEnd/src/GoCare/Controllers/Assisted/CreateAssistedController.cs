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
public sealed class CreateAssistedController(AssistedPersonService assistedPeople) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(AssistedPersonRequest request, CancellationToken ct)
    {
        var caregiverId = Guid.Parse(User.FindFirstValue("sub")!);

        var assistedPersonId = await assistedPeople.CreateAsync(
            caregiverId,
            request.Name,
            request.Surname,
            request.BirthDate,
            request.Phone,
            request.HomeAddress?.ToAddress(),
            ct);

        var created = await assistedPeople.GetDetailAsync(caregiverId, assistedPersonId, ct);

        return Created($"/assisted/{assistedPersonId}", created);
    }
}
