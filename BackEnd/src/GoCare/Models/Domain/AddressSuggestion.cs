namespace GoCare.Models.Domain;

public sealed record AddressSuggestion(
    string DisplayName,
    string Street,
    string Number,
    string PostalCode,
    string City,
    string Province,
    double Latitude,
    double Longitude);
