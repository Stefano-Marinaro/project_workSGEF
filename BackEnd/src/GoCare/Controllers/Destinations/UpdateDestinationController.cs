using System.Security.Claims;

using GoCare.Dtos.Destinations.Requests;
using GoCare.Dtos.Transport.Requests;
using GoCare.Models.Enums;
using GoCare.Services.Destinations;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Destinations;

[ApiController]
[Route("me/destinations")]
[Tags("Destinazioni")]
[Authorize(Roles = nameof(EAccountRole.Person))]
public sealed class UpdateDestinationController(UpdateDestinationService service) : ControllerBase
{
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, SavedDestinationRequest request, CancellationToken ct)
    {
        var personId = Guid.Parse(User.FindFirstValue("sub")!);

        await service.UpdateAsync(personId, id, request.PlaceName, request.SavedAddress.ToAddress(), request.Note, ct);

        return NoContent();
    }
}
