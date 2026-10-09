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
public sealed class CreateDestinationController(CreateDestinationService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(SavedDestinationRequest request, CancellationToken ct)
    {
        var personId = Guid.Parse(User.FindFirstValue("sub")!);

        var created = await service.CreateAsync(
            personId, request.PlaceName, request.SavedAddress.ToAddress(), request.Note, ct);

        return Created($"/me/destinations/{created.Id}", created);
    }
}
