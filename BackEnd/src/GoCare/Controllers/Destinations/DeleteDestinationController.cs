using System.Security.Claims;

using GoCare.Models.Enums;
using GoCare.Services.Destinations;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Destinations;

[ApiController]
[Route("me/destinations")]
[Tags("Destinazioni")]
[Authorize(Roles = nameof(EAccountRole.Person))]
public sealed class DeleteDestinationController(DeleteDestinationService service) : ControllerBase
{
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var personId = Guid.Parse(User.FindFirstValue("sub")!);

        await service.DeleteAsync(personId, id, ct);

        return NoContent();
    }
}
