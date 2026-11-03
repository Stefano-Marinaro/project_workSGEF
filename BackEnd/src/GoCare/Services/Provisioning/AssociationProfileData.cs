using GoCare.Models.Domain;

namespace GoCare.Services.Provisioning;

public sealed record AssociationProfileData(
       string Name, TripAddress Headquarter, List<string> Phones, List<string> CoveredComuni);
