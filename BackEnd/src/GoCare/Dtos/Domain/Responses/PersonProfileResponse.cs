using GoCare.Models.Domain;

namespace GoCare.Dtos.Domain.Responses;

public sealed record PersonProfileResponse(
    string? Name,
    string? Surname,
    DateOnly? BirthDate,
    string? Phone,
    Address? HomeAddress,
    string Email);
