using GoCare.Models.Domain;

namespace GoCare.Services.Provisioning;

public sealed record AssociationProfileData(
       string Name, Address Headquarter, List<string> Phones, List<string> CoveredProvinces);
