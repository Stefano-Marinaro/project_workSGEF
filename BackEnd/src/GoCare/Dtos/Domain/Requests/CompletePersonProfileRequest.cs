namespace GoCare.Dtos.Domain.Requests;

public sealed record CompletePersonProfileRequest(
    string Name,
    string Surname,
    DateOnly BirthDate,
    string Phone);
