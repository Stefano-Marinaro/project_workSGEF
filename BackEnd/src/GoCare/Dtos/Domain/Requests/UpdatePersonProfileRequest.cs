using GoCare.Dtos.Transport.Requests;

namespace GoCare.Dtos.Domain.Requests;

// descrive il JSON che il frontend deve mandare
public sealed record UpdatePersonProfileRequest(
    string Name,
    string Surname,
    DateOnly BirthDate,
    string Phone,
    AddressRequest? HomeAddress
);