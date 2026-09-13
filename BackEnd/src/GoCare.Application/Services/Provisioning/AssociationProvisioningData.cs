using GoCare.Application.Models.Domain;

namespace GoCare.Application.Services.Provisioning;

public sealed record AssociationProvisioningData(
       string Name, Address Headquarter, List<string> Phones, string Email, List<string> CoveredProvinces);
