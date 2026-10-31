using GoCare.Data;
using GoCare.Errors;
using GoCare.Models.Auth;
using GoCare.Models.Domain;
using GoCare.Models.Enums;
using GoCare.Services.Admin;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace GoCare.Tests.Services.Admin;

public class RestoreCaregiverAccountServiceTests
{
    private static RestoreCaregiverAccountService NewService(GoCareDbContext db) =>
        new(db, NullLogger<RestoreCaregiverAccountService>.Instance);

    private static async Task<Account> SeedCaregiver(GoCareDbContext db, Action<Account>? change = null)
    {
        var account = TestSupport.NewAccount(EAccountRole.Person, "mario@example.com");
        change?.Invoke(account);
        db.Accounts.Add(account);
        db.Persons.Add(new Person(account.Id, account.Email));
        await db.SaveChangesAsync();
        return account;
    }

    [Fact]
    public async Task Restore_ReinstatesSuspendedAccount_AndClearsFailedLogins()
    {
        using var db = TestSupport.NewDb();
        var account = await SeedCaregiver(db, a => a.Suspend(DateTimeOffset.UtcNow));
        db.FailedLoginAttempts.Add(new FailedLoginAttempt(Guid.NewGuid(), account.Email, DateTimeOffset.UtcNow, null));
        await db.SaveChangesAsync();

        await NewService(db).RestoreAsync(account.Id, Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(EAccountStatus.Active, (await db.Accounts.SingleAsync()).Status);
        Assert.Empty(db.FailedLoginAttempts);
    }

    [Fact]
    public async Task Restore_RestoresDeletedAccount_WhenNotAnonymized()
    {
        using var db = TestSupport.NewDb();
        var account = await SeedCaregiver(db, a => a.MarkDeleted(DateTimeOffset.UtcNow));

        await NewService(db).RestoreAsync(account.Id, Guid.NewGuid(), CancellationToken.None);

        var saved = await db.Accounts.SingleAsync();
        Assert.Equal(EAccountStatus.Active, saved.Status);
        Assert.Null(saved.DeletedAt);
    }

    [Fact]
    public async Task Restore_UnlocksActiveAccount_WithFailedLogins()
    {
        using var db = TestSupport.NewDb();
        var account = await SeedCaregiver(db);
        db.FailedLoginAttempts.Add(new FailedLoginAttempt(Guid.NewGuid(), account.Email, DateTimeOffset.UtcNow, null));
        await db.SaveChangesAsync();

        await NewService(db).RestoreAsync(account.Id, Guid.NewGuid(), CancellationToken.None);

        Assert.Empty(db.FailedLoginAttempts);
    }

    [Fact]
    public async Task Restore_Throws_WhenNothingToRestore()
    {
        using var db = TestSupport.NewDb();
        var account = await SeedCaregiver(db);

        await Assert.ThrowsAsync<ConflictException>(() =>
            NewService(db).RestoreAsync(account.Id, Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task Restore_Throws_ForAnAssociationAccount()
    {
        using var db = TestSupport.NewDb();
        var association = TestSupport.NewAccount(EAccountRole.Association, "croce@example.com");
        association.Suspend(DateTimeOffset.UtcNow);
        db.Accounts.Add(association);
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            NewService(db).RestoreAsync(association.Id, Guid.NewGuid(), CancellationToken.None));
    }
}
