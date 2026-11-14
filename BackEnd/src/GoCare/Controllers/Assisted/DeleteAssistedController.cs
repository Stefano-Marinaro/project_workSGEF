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
public sealed class DeleteAssistedController(AssistedPersonService assistedPeople) : ControllerBase
{
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var callerId = Guid.Parse(User.FindFirstValue("sub")!);
        await assistedPeople.RemoveFromListAsync(callerId, id, ct);
        return NoContent();
    }
}
