using System.Security.Claims;

using GoCare.Models.Enums;
using GoCare.Services.Domain;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Assisted;

[ApiController]
[Route("assisted")]
[Tags("Assistiti")]
[Authorize(Roles = nameof(EAccountRole.Person))]
public sealed class RemoveCaregiverController(AssistedPersonService assistedPeople) : ControllerBase
{
    [HttpDelete("{id:guid}/caregivers/{caregiverId:guid}")]
    public async Task<IActionResult> RemoveCaregiver(Guid id, Guid caregiverId, CancellationToken ct)
    {
        var callerId = Guid.Parse(User.FindFirstValue("sub")!);

        await assistedPeople.RemoveCaregiverAsync(callerId, id, caregiverId, ct);

        return NoContent();
    }
}
