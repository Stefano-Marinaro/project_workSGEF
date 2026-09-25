using System.Security.Claims;

using GoCare.Dtos.Domain.Requests;
using GoCare.Errors;
using GoCare.Models.Domain;
using GoCare.Models.Enums;
using GoCare.Services.Provisioning;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Domain;

[ApiController]
public sealed class ProfilesController(
    ProfileProvisioningService provisioning) : ControllerBase
{
    [Authorize(Roles = nameof(EAccountRole.Person))]
    [HttpPatch("/me/profile")]
    public async Task<IActionResult> CompletePersonProfile(
        CompletePersonProfileRequest request,
        CancellationToken ct)
    {
        await provisioning.CompletePersonProfileAsync(
            GetAccountId(),
            new PersonProfileData(
                request.Name,
                request.Surname,
                request.BirthDate,
                request.Phone),
            ct);

        return NoContent();
    }

    [Authorize(Roles = nameof(EAccountRole.Association))]
    [HttpPatch("/association/profile")]
    public async Task<IActionResult> CompleteAssociationProfile(
        CompleteAssociationProfileRequest request,
        CancellationToken ct)
    {
        var address = new Address(
            request.Headquarter.Street,
            request.Headquarter.Number,
            request.Headquarter.PostalCode,
            request.Headquarter.City,
            request.Headquarter.Province);

        await provisioning.CompleteAssociationProfileAsync(
            GetAccountId(),
            new AssociationProfileData(
                request.Name,
                address,
                request.Phones,
                request.CoveredProvinces),
            ct);

        return NoContent();
    }

    private Guid GetAccountId()
    {
        var subject = User.FindFirstValue("sub");

        if (!Guid.TryParse(subject, out var accountId))
            throw new ForbiddenException("Il token non contiene un identificativo account valido.");

        return accountId;
    }
}
