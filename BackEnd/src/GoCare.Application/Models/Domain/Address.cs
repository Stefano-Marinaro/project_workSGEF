namespace GoCare.Application.Models.Domain;

public sealed record Address(
    string Street,
    string Number,
    string PostalCode,
    string City,
    string Province
    );
