namespace GoCare.Dtos.Transport.Requests;

public sealed record TripAddressRequest(
    string Street,
    string Number,
    string PostalCode,
    string City,
    string Province,
    double Latitude,
    double Longitude);
