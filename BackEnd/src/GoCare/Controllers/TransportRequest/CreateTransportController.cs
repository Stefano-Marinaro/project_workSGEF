using System.Security.Claims;

using GoCare.Dtos.Transport.Requests;
using GoCare.Dtos.Transport.Responses;
using GoCare.Models.Domain;
using GoCare.Models.Enums;
using GoCare.Services.Transport;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.TransportRequest;

[ApiController]
[Route("transports")]
[Tags("Trasporti")]
[Authorize(Roles = nameof(EAccountRole.Person))]
public sealed class CreateTransportController(CreateTransportService transportService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateTransport(NewTransportRequest request, CancellationToken ct)
    {
        var requestedById = Guid.Parse(User.FindFirstValue("sub")!);

        var startAddress = ToAddress(request.StartAddress);
        var endAddress = ToAddress(request.EndAddress);
        var returnEndAddress = request.ReturnEndAddress is null ? null : ToAddress(request.ReturnEndAddress);

        var companions = (request.Companions ?? [])
            .Select(c => (c.Name, c.Surname, c.Relationship, c.Phone))
            .ToList();

        var id = await transportService.CreateTransportAsync(
            requestedById,
            request.BeneficiaryId,
            request.TripType,
            request.TripDirection,
            request.DepartureDateHour,
            request.ReturnDateHour,
            startAddress,
            endAddress,
            returnEndAddress,
            request.ReferencePhone,
            request.ReferenceEmail,
            companions,
            ct);

        return Created($"/transports/{id}", new CreateTransportResponse(id));
    }

    private static Address ToAddress(AddressRequest a) =>
        new(a.Street, a.Number, a.PostalCode, a.City, a.Province);
}
