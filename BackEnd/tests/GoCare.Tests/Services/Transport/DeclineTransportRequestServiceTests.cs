using GoCare.Data;
using GoCare.Errors;
using GoCare.Models.Domain;
using GoCare.Models.Enums;
using GoCare.Services.Transport;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Tests.Services.Transport;

public class DeclineTransportRequestServiceTests
{
    private static DeclineTransportRequestService NewService(GoCareDbContext db) =>
        new(db, new CoverageEvaluator(db), new NotificationDispatcher(db, new FakeEmailSender(), new FakePushSender()));

    private static async Task<(TransportRequest Transport, Guid First, Guid Second)> SeedTwoCandidates(GoCareDbContext db)
    {
        var transport = TestSupport.NewTransport(Guid.NewGuid());
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        db.TransportRequests.Add(transport);
        db.TransportRequestCandidates.AddRange(
            new TransportRequestCandidate(transport.Id, first),
            new TransportRequestCandidate(transport.Id, second));
        await db.SaveChangesAsync();
        return (transport, first, second);
    }

    [Fact]
    public async Task Decline_RecordsRejection_AndKeepsRequestPending_WhenOtherCandidatesRemain()
    {
        using var db = TestSupport.NewDb();
        var (transport, first, _) = await SeedTwoCandidates(db);

        await NewService(db).DeclineAsync(transport.Id, first, "  Nessun mezzo  ", CancellationToken.None);

        var rejection = await db.TransportRequestRejections.SingleAsync();
        Assert.Equal(ERejectionKind.Declined, rejection.Kind);
        Assert.Equal("Nessun mezzo", rejection.Reason);
        Assert.Equal(ETripRequestStatus.Pending, (await db.TransportRequests.SingleAsync()).RequestStatus);
    }

    [Fact]
    public async Task Decline_MarksNotCovered_AndNotifiesCaregiver_WhenLastCandidateDeclines()
    {
        using var db = TestSupport.NewDb();
        var (transport, first, second) = await SeedTwoCandidates(db);
        var service = NewService(db);

        await service.DeclineAsync(transport.Id, first, null, CancellationToken.None);
        await service.DeclineAsync(transport.Id, second, null, CancellationToken.None);

        var saved = await db.TransportRequests.SingleAsync();
        Assert.Equal(ETripRequestStatus.NotCovered, saved.RequestStatus);
        Assert.NotNull(saved.NotCoveredAt);
        var notification = await db.Notifications.SingleAsync();
        Assert.Equal(ENotificationType.RequestNotCovered, notification.Type);
        Assert.Equal(transport.RequestedById, notification.SubjectId);
    }

    [Fact]
    public async Task Decline_Throws_WhenAlreadyDeclined()
    {
        using var db = TestSupport.NewDb();
        var (transport, first, _) = await SeedTwoCandidates(db);
        var service = NewService(db);
        await service.DeclineAsync(transport.Id, first, null, CancellationToken.None);

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.DeclineAsync(transport.Id, first, null, CancellationToken.None));
    }

    [Fact]
    public async Task Decline_Throws_WhenAssociationIsNotCandidate()
    {
        using var db = TestSupport.NewDb();
        var (transport, _, _) = await SeedTwoCandidates(db);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            NewService(db).DeclineAsync(transport.Id, Guid.NewGuid(), null, CancellationToken.None));
    }

    [Fact]
    public async Task Decline_Throws_WhenReasonIsTooLong()
    {
        using var db = TestSupport.NewDb();
        var (transport, first, _) = await SeedTwoCandidates(db);

        await Assert.ThrowsAsync<ValidationException>(() =>
            NewService(db).DeclineAsync(transport.Id, first, new string('x', 301), CancellationToken.None));
    }
}
