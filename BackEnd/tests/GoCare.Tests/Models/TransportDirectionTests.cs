using GoCare.Models.Domain;
using GoCare.Models.Enums;
using GoCare.Dtos.Transport.Requests;
using GoCare.Dtos.Transport.Validators;
using FluentValidation;

namespace GoCare.Tests.Models;

public class TransportDirectionTests
{
    private static TripAddress Address() => new("Via Roma", "1", "06100", "Perugia", "PG", 43.11, 12.39);

    [Theory]
    [InlineData(ETripType.Hospitalization)]
    [InlineData(ETripType.Discharge)]
    [InlineData(ETripType.Transfer)]
    public void NonVisitTransport_RejectsRoundTrip_InDomain(ETripType type)
    {
        var departure = DateTimeOffset.UtcNow.AddDays(1);
        Assert.Throws<InvalidOperationException>(() => new TransportRequest(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), type, ETripDirection.RoundTrip,
            departure, departure.AddHours(2), Address(), Address(), null, "3331234567", null, 10, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Visit_AllowsRoundTrip_WithImplicitReturnToStart()
    {
        var departure = DateTimeOffset.UtcNow.AddDays(1);
        var trip = new TransportRequest(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), ETripType.Visit,
            ETripDirection.RoundTrip, departure, departure.AddHours(2), Address(), Address(), null,
            "3331234567", null, 10, DateTimeOffset.UtcNow);
        Assert.Null(trip.ReturnEndAddress);
    }

    [Fact]
    public void VisitRoundTripCanReturnAfterVisitWithoutScheduledTime()
    {
        var trip = new TransportRequest(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), ETripType.Visit,
            ETripDirection.RoundTrip, DateTimeOffset.UtcNow.AddDays(1), null, Address(), Address(), null,
            "3331234567", null, 10, DateTimeOffset.UtcNow);
        Assert.Null(trip.ReturnDateHour);
    }

    [Fact]
    public async Task ReturnTimeValidatorAllowsUnscheduledRoundTripAndStillRejectsInvalidTimes()
    {
        using var db = TestSupport.NewDb();
        var validator = new NewTransportValidator(db);
        var departure = DateTimeOffset.UtcNow.AddDays(1);
        var address = new TripAddressRequest("Via Roma", "1", "06121", "Perugia", "PG", 43.11, 12.39);
        var request = new NewTransportRequest(Guid.NewGuid(), ETripType.Visit, ETripDirection.RoundTrip,
            departure, null, address, address, null, "3331234567", null, []);
        var unscheduled = await validator.ValidateAsync(request, options => options.IncludeProperties(x => x.ReturnDateHour));
        var beforeDeparture = await validator.ValidateAsync(request with { ReturnDateHour = departure.AddHours(-1) }, options => options.IncludeProperties(x => x.ReturnDateHour));
        var oneWay = await validator.ValidateAsync(request with { TripDirection = ETripDirection.OneWay, ReturnDateHour = departure.AddHours(1) }, options => options.IncludeProperties(x => x.ReturnDateHour));
        Assert.True(unscheduled.IsValid);
        Assert.False(beforeDeparture.IsValid);
        Assert.False(oneWay.IsValid);
    }
}
