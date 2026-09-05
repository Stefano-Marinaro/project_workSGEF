namespace GoCare.Application.Models.Domain;

public sealed record Address(
    string Street,
    string Number,
    string Cap,
    string City,
    string Province
    );
