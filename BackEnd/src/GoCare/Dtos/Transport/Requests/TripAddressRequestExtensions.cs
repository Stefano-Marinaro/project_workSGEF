using GoCare.Models.Domain;

namespace GoCare.Dtos.Transport.Requests;

public static class TripAddressRequestExtensions
{
    public static TripAddress ToTripAddress(this TripAddressRequest a) =>
        new(a.Street, a.Number, a.PostalCode, a.City, a.Province, a.Latitude, a.Longitude);
}
