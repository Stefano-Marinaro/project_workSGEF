namespace GoCare.Dtos.Transport.Requests;

public sealed record AddressRequest(string Street, string Number, string PostalCode, string City, string Province);
