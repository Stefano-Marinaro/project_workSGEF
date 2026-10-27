using GoCare.Data;
using GoCare.Errors;
using GoCare.Models.Domain;
using GoCare.Models.Enums;
using GoCare.Services.Transport;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Tests.Services.Transport;

public class CancelAcceptedTransportServiceTests
{
    private static CancelAcceptedTransportService NewService(GoCareDbContext db) =>
        new(db, new CoverageEvaluator(db), new NotificationDispatcher(db, new FakeEmailSender(), new FakePushSender()));

    // richiesta con due candidate, accettata dalla prima
    private static async Task<(TransportRequest Transport, Guid Accepted, Guid Other)> SeedAccepted(GoCareDbContext db)
    {
        var transport = TestSupport.NewTransport(Guid.NewGuid());
        var accepted = Guid.NewGuid();
        var other = Guid.NewGuid();
        transport.Accept(accepted, DateTimeOffset.UtcNow);
        db.TransportRequests.Add(transport);
        db.TransportRequestCandidates.AddRange(
            new TransportRequestCandidate(transport.Id, accepted),
            new TransportRequestCandidate(transport.Id, other));
        db.TripOperatorLinks.Add(new TripOperatorLink(
            Guid.NewGuid(), transport.Id, "token", DateTimeOffset.UtcNow.AddDays(3), DateTimeOffset.UtcNow));
        await db.SaveChangesAsync();
        return (transport, accepted, other);
    }

    [Fact]
    public async Task Cancel_ReleasesRequest_RevokesOperatorLinks_AndNotifiesCaregiver()
    {
        using var db = TestSupport.NewDb();
        var (transport, accepted, _) = await SeedAccepted(db);

        await NewService(db).CancelAsync(transport.Id, accepted, "Mezzo guasto", CancellationToken.None);

        var saved = await db.TransportRequests.SingleAsync();
        Assert.Equal(ETripRequestStatus.Pending, saved.RequestStatus);
        Assert.Null(saved.AssignedAssociationId);
        Assert.Null(saved.AcceptedAt);

        var rejection = await db.TransportRequestRejections.SingleAsync();
        Assert.Equal(ERejectionKind.CancelledAfterAcceptance, rejection.Kind);
        Assert.Equal("Mezzo guasto", rejection.Reason);

        Assert.NotNull((await db.TripOperatorLinks.SingleAsync()).RevokedAt);
        Assert.Equal(ENotificationType.TripCancelledByAssociation, (await db.Notifications.SingleAsync()).Type);
    }

    [Fact]
    public async Task Cancel_MarksNotCovered_WhenNoOtherCandidateIsLeft()
    {
        using var db = TestSupport.NewDb();
        var (transport, accepted, other) = await SeedAccepted(db);
        db.TransportRequestRejections.Add(new TransportRequestRejection(
            Guid.NewGuid(), transport.Id, other, ERejectionKind.Declined, null, DateTimeOffset.UtcNow));
        await db.SaveChangesAsync();

        await NewService(db).CancelAsync(transport.Id, accepted, "Mezzo guasto", CancellationToken.None);

        Assert.Equal(ETripRequestStatus.NotCovered, (await db.TransportRequests.SingleAsync()).RequestStatus);
    }

    [Fact]
    public async Task Cancel_Throws_WhenTransportIsAssignedToAnotherAssociation()
    {
        using var db = TestSupport.NewDb();
        var (transport, _, other) = await SeedAccepted(db);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            NewService(db).CancelAsync(transport.Id, other, "Motivo", CancellationToken.None));
    }

    [Fact]
    public async Task Cancel_Throws_WhenTripHasAlreadyStarted()
    {
        using var db = TestSupport.NewDb();
        var (transport, accepted, _) = await SeedAccepted(db);
        transport.StartExecution();
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<ConflictException>(() =>
            NewService(db).CancelAsync(transport.Id, accepted, "Motivo", CancellationToken.None));
    }
}
