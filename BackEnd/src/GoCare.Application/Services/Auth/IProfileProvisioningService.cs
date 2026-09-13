using GoCare.Application.Services.Provisioning;

namespace GoCare.Application.Services.Auth;

public interface IProfileProvisioningService
{
    Task CreatePersonAsync(Guid accountId, PersonProvisioningData data, CancellationToken ct);
    Task CreateAssociationAsync(Guid accountId, AssociationProvisioningData data, CancellationToken ct);
}
