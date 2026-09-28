using GoCare.Models.Domain;

namespace GoCare.Dtos.Transport.Requests;

public static class AddressRequestExtensions
{
    public static Address ToAddress(this AddressRequest a) =>
        new(a.Street, a.Number, a.PostalCode, a.City, a.Province);
}
