using System.Security.Claims;

using GoCare.Dtos.Domain.Requests;
using GoCare.Models.Enums;
using GoCare.Services.Domain;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Assisted;

[ApiController]
[Route("assisted")]
[Tags("Assistiti")]
[Authorize(Roles = nameof(EAccountRole.Person))]
public sealed class AddCaregiverController(AssistedPersonService assistedPeople) : ControllerBase
{
    [HttpPost("{id:guid}/caregivers")]
    public async Task<IActionResult> AddCaregiver(Guid id, AddCaregiverRequest request, CancellationToken ct)
    {
        var caregiverId = Guid.Parse(User.FindFirstValue("sub")!);

        await assistedPeople.AddCaregiverAsync(caregiverId, id, request.Email, ct);

        return NoContent();
    }
}
