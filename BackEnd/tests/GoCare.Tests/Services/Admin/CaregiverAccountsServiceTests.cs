using GoCare.Data;
using GoCare.Errors;
using GoCare.Models.Auth;
using GoCare.Models.Domain;
using GoCare.Models.Enums;
using GoCare.Pagination;
using GoCare.Services.Admin;

namespace GoCare.Tests.Services.Admin;

public class CaregiverAccountsServiceTests
{
    private static async Task<Account> SeedCaregiver(GoCareDbContext db, string email, string surname, bool verified = true)
    {
        var account = TestSupport.NewAccount(EAccountRole.Person, email, verified);
        var person = new Person(account.Id, email);
        person.CompleteProfile("Mario", surname, new DateOnly(1970, 1, 1), "333111222");
        db.Accounts.Add(account);
        db.Persons.Add(person);
        await db.SaveChangesAsync();
        return account;
    }

    // la ricerca testuale usa ILike di PostgreSQL, che il database in memoria non supporta: qui si provano stato e ordine
    [Fact]
    public async Task List_FiltersByStatus_AndExcludesAssociations()
    {
        using var db = TestSupport.NewDb();
        await SeedCaregiver(db, "bianchi@example.com", "Bianchi");
        await SeedCaregiver(db, "verdi@example.com", "Verdi", verified: false);
        db.Accounts.Add(TestSupport.NewAccount(EAccountRole.Association, "croce@example.com"));
        await db.SaveChangesAsync();
        var service = new ListCaregiverAccountsService(db);

        var all = await service.ListAsync(null, null, new PageQuery(), CancellationToken.None);
        var unverified = await service.ListAsync(null, EAccountStatus.Unverified, new PageQuery(), CancellationToken.None);

        Assert.Equal(["Bianchi", "Verdi"], all.Items.Select(i => i.Surname!));
        Assert.Equal("verdi@example.com", Assert.Single(unverified.Items).Email);
    }

    [Fact]
    public async Task Detail_ReturnsAccountData_AndCounters()
    {
        using var db = TestSupport.NewDb();
        var account = await SeedCaregiver(db, "bianchi@example.com", "Bianchi");
        db.FailedLoginAttempts.Add(new FailedLoginAttempt(Guid.NewGuid(), "bianchi@example.com", DateTimeOffset.UtcNow, null));
        db.CaregiverAssistedLinks.Add(new CaregiverAssistedLink(account.Id, Guid.NewGuid(), DateTimeOffset.UtcNow));
        db.TransportRequests.Add(TestSupport.NewTransport(account.Id));
        await db.SaveChangesAsync();

        var detail = await new CaregiverAccountDetailService(db).GetAsync(account.Id, CancellationToken.None);

        Assert.Equal("Bianchi", detail.Surname);
        Assert.Equal(EAccountStatus.Active, detail.AccountStatus);
        Assert.Equal(1, detail.FailedLoginAttempts);
        Assert.Equal(1, detail.LinkedAssistedPeople);
        Assert.Equal(1, detail.TransportRequests);
    }

    [Fact]
    public async Task Detail_Throws_ForAnAssociationAccount()
    {
        using var db = TestSupport.NewDb();
        var association = TestSupport.NewAccount(EAccountRole.Association, "croce@example.com");
        db.Accounts.Add(association);
        db.Persons.Add(new Person(association.Id, association.Email)); // anche se esistesse un profilo persona
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            new CaregiverAccountDetailService(db).GetAsync(association.Id, CancellationToken.None));
    }
}
