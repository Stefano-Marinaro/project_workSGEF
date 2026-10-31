using GoCare.Errors;
using GoCare.Models.Auth;
using GoCare.Models.Domain;
using GoCare.Models.Enums;
using GoCare.Services.Admin;

namespace GoCare.Tests.Services.Admin;

public class PartnerAssociationDetailServiceTests
{
    [Fact]
    public async Task Get_ReturnsProfile_AccountStatus_AndAssignedTransports()
    {
        using var db = TestSupport.NewDb();
        var account = new Account(Guid.NewGuid(), "croce@example.com", "hash", EAccountRole.Association, DateTimeOffset.UtcNow);
        var association = new Association(account.Id, account.Email);
        association.UpdateProfile("Croce Verde", TestSupport.SampleAddress(), ["075123456"], "Lun-Ven 8-18");
        db.Accounts.Add(account);
        db.Associations.Add(association);
        db.Comuni.Add(new Comune(Guid.NewGuid(), "054039", "Perugia", "PG"));
        db.AssociationCoveredComuni.Add(new AssociationCoveredComune(account.Id, "054039"));

        var active = TestSupport.NewTransport(Guid.NewGuid());
        active.Accept(account.Id, DateTimeOffset.UtcNow);
        var completed = TestSupport.NewTransport(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(-3));
        completed.Accept(account.Id, DateTimeOffset.UtcNow);
        completed.StartExecution();
        completed.CompleteExecution();
        var otherAssociation = TestSupport.NewTransport(Guid.NewGuid());
        otherAssociation.Accept(Guid.NewGuid(), DateTimeOffset.UtcNow);
        db.TransportRequests.AddRange(active, completed, otherAssociation);
        await db.SaveChangesAsync();

        var detail = await new PartnerAssociationDetailService(db).GetAsync(account.Id, CancellationToken.None);

        Assert.Equal("Croce Verde", detail.Name);
        Assert.Equal(EAccountStatus.Unverified, detail.AccountStatus);
        Assert.Equal("Perugia", Assert.Single(detail.CoveredComuni).Nome);
        Assert.Equal(active.Id, Assert.Single(detail.ActiveTransports).Id);
        Assert.Equal(completed.Id, Assert.Single(detail.RecentTransports).Id);
        Assert.Equal("Perugia", detail.ActiveTransports[0].StartCity);
    }

    [Fact]
    public async Task Get_Throws_WhenAssociationDoesNotExist()
    {
        using var db = TestSupport.NewDb();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            new PartnerAssociationDetailService(db).GetAsync(Guid.NewGuid(), CancellationToken.None));
    }
}
