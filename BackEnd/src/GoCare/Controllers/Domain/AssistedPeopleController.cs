using System.Security.Claims;

using GoCare.Dtos.Domain.Requests;
using GoCare.Dtos.Domain.Responses;
using GoCare.Errors;
using GoCare.Models.Domain;
using GoCare.Models.Enums;
using GoCare.Services.Domain;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Domain;

[ApiController]
[Authorize(Roles = nameof(EAccountRole.Person))]
[Route("assisted")]
public sealed class AssistedPeopleController(AssistedPersonService assistedPeople) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<AssistedPersonDetailResponse>> Create(
        AssistedPersonRequest request,
        CancellationToken ct)
    {
        var caregiverId = GetAccountId();
        var assistedPersonId = await assistedPeople.CreateAsync(
            caregiverId,
            request.Name,
            request.Surname,
            request.BirthDate,
            request.Phone,
            ToAddress(request.HomeAddress),
            ct);

        var created = await assistedPeople.GetDetailAsync(caregiverId, assistedPersonId, ct);
        return CreatedAtAction(nameof(GetDetail), new { id = assistedPersonId }, ToResponse(created));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AssistedPersonSummaryResponse>>> List(CancellationToken ct)
    {
        var result = await assistedPeople.ListAsync(GetAccountId(), ct);
        return Ok(result.Select(ToResponse).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AssistedPersonDetailResponse>> GetDetail(
        Guid id,
        CancellationToken ct)
    {
        var result = await assistedPeople.GetDetailAsync(GetAccountId(), id, ct);
        return Ok(ToResponse(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        AssistedPersonRequest request,
        CancellationToken ct)
    {
        await assistedPeople.UpdateAsync(
            GetAccountId(),
            id,
            request.Name,
            request.Surname,
            request.BirthDate,
            request.Phone,
            ToAddress(request.HomeAddress),
            ct);

        return NoContent();
    }

    [HttpPost("{id:guid}/caregivers")]
    public async Task<IActionResult> AddCaregiver(
        Guid id,
        AddCaregiverRequest request,
        CancellationToken ct)
    {
        await assistedPeople.AddCaregiverAsync(GetAccountId(), id, request.Email, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}/caregivers/{caregiverId:guid}")]
    public async Task<IActionResult> RemoveCaregiver(
        Guid id,
        Guid caregiverId,
        CancellationToken ct)
    {
        await assistedPeople.RemoveCaregiverAsync(GetAccountId(), id, caregiverId, ct);
        return NoContent();
    }

    private Guid GetAccountId()
    {
        var subject = User.FindFirstValue("sub");

        if (!Guid.TryParse(subject, out var accountId))
            throw new ForbiddenException("Il token non contiene un identificativo account valido.");

        return accountId;
    }

    private static Address? ToAddress(Dtos.Transport.Requests.AddressRequest? address) =>
        address is null
            ? null
            : new Address(address.Street, address.Number, address.PostalCode, address.City, address.Province);

    private static AddressResponse? ToResponse(Address? address) =>
        address is null
            ? null
            : new AddressResponse(
                address.Street,
                address.Number,
                address.PostalCode,
                address.City,
                address.Province);

    private static AssistedPersonSummaryResponse ToResponse(AssistedPersonSummary person) => new(
        person.Id,
        person.Name,
        person.Surname,
        person.BirthDate,
        person.Phone,
        ToResponse(person.HomeAddress),
        person.NextTrip is null ? null : ToResponse(person.NextTrip));

    private static AssistedPersonDetailResponse ToResponse(AssistedPersonDetail person) => new(
        person.Id,
        person.Name,
        person.Surname,
        person.BirthDate,
        person.Phone,
        ToResponse(person.HomeAddress),
        person.Caregivers
            .Select(caregiver => new CaregiverResponse(
                caregiver.Id,
                caregiver.Name,
                caregiver.Surname,
                caregiver.Email))
            .ToList(),
        person.Trips.Select(ToResponse).ToList());

    private static AssistedTripResponse ToResponse(AssistedTripSummary trip) => new(
        trip.Id,
        trip.RequestedById,
        trip.TripType,
        trip.TripDirection,
        trip.DepartureDateHour,
        trip.ReturnDateHour,
        trip.Status);
}
