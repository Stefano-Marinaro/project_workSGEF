using GoCare.Dtos.Transport.Requests;

namespace GoCare.Dtos.Domain.Requests;

public sealed record AssistedPersonRequest(
    string Name,
    string Surname,
    DateOnly BirthDate,
    string Phone,
    AddressRequest? HomeAddress);
