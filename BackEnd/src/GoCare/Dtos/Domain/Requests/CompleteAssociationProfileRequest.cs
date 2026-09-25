namespace GoCare.Dtos.Domain.Requests;

public sealed record CompleteAssociationProfileRequest(
    string Name,
    AddressRequest Headquarter,
    List<string> Phones,
    List<string> CoveredProvinces);

public sealed record AddressRequest(
    string Street,
    string Number,
    string PostalCode,
    string City,
    string Province);
