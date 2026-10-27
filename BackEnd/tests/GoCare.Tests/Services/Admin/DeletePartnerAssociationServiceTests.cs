using GoCare.Data;
using GoCare.Errors;
using GoCare.Models.Auth;
using GoCare.Models.Domain;
using GoCare.Models.Enums;
using GoCare.Services.Admin;
using GoCare.Services.Transport;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Tests.Services.Admin;

public class DeletePartnerAssociationServiceTests
{
    private static DeletePartnerAssociationService NewService(GoCareDbContext db) =>
        new(db, new CoverageEvaluator(db), new NotificationDispatcher(db, new FakeEmailSender(), new FakePushSender()));

    private static async Task<Account> SeedAssociation(GoCareDbContext db)
    {
        var account = TestSupport.NewAccount(EAccountRole.Association, "croce@example.com");
        db.Accounts.Add(account);
        db.Associations.Add(new Association(account.Id, account.Email));
        db.RefreshTokens.Add(new RefreshToken(
            Guid.NewGuid(), account.Id, "sessione", DateTimeOffset.UtcNow.AddDays(7), DateTimeOffset.UtcNow));
        await db.SaveChangesAsync();
        return account;
    }

    [Fact]
    public async Task Delete_SoftDeletes_RevokesSessions_AndReleasesPendingRequests()
    {
        using var db = TestSupport.NewDb();
        var account = await SeedAssociation(db);
        var otherCandidate = Guid.NewGuid();

        // richiesta in cui era l'unica candidata → diventa NotCovered
        var onlyHer = TestSupport.NewTransport(Guid.NewGuid());
        // richiesta con un'altra candidata → resta Pending
        var shared = TestSupport.NewTransport(Guid.NewGuid());
        db.TransportRequests.AddRange(onlyHer, shared);
        db.TransportRequestCandidates.AddRange(
            new TransportRequestCandidate(onlyHer.Id, account.Id),
            new TransportRequestCandidate(shared.Id, account.Id),
            new TransportRequestCandidate(shared.Id, otherCandidate));
        await db.SaveChangesAsync();

        await NewService(db).DeleteAsync(account.Id, CancellationToken.None);

        Assert.Equal(EAccountStatus.Deleted, (await db.Accounts.SingleAsync()).Status);
        Assert.NotNull((await db.Associations.SingleAsync()).DeletedAt);
        Assert.NotNull((await db.RefreshTokens.SingleAsync()).RevokedAt);

        Assert.Equal(ETripRequestStatus.NotCovered, (await db.TransportRequests.SingleAsync(t => t.Id == onlyHer.Id)).RequestStatus);
        Assert.Equal(ETripRequestStatus.Pending, (await db.TransportRequests.SingleAsync(t => t.Id == shared.Id)).RequestStatus);
        Assert.Equal(2, await db.TransportRequestRejections.CountAsync(r => r.AssociationId == account.Id));

        var notification = await db.Notifications.SingleAsync();
        Assert.Equal(onlyHer.RequestedById, notification.SubjectId);
    }

    [Fact]
    public async Task Delete_Throws_WhenAssociationHasActiveTransports()
    {
        using var db = TestSupport.NewDb();
        var account = await SeedAssociation(db);
        var transport = TestSupport.NewTransport(Guid.NewGuid());
        transport.Accept(account.Id, DateTimeOffset.UtcNow);
        db.TransportRequests.Add(transport);
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<ConflictException>(() => NewService(db).DeleteAsync(account.Id, CancellationToken.None));
        Assert.Equal(EAccountStatus.Active, (await db.Accounts.SingleAsync()).Status);
    }

    [Fact]
    public async Task Delete_Throws_WhenAlreadyDeleted()
    {
        using var db = TestSupport.NewDb();
        var account = await SeedAssociation(db);
        var service = NewService(db);
        await service.DeleteAsync(account.Id, CancellationToken.None);

        await Assert.ThrowsAsync<ConflictException>(() => service.DeleteAsync(account.Id, CancellationToken.None));
    }
}
