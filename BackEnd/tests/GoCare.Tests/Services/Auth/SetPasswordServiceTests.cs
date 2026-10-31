using GoCare.Errors;
using GoCare.Models.Auth;
using GoCare.Models.Enums;
using GoCare.Services.Auth;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Tests.Services.Auth;

public class SetPasswordServiceTests
{
    [Fact]
    public async Task SetPassword_ChangesPassword_AndActivatesInvitedAccount()
    {
        using var db = TestSupport.NewDb();
        var account = TestSupport.NewAccount(EAccountRole.Association, "croce@example.com", verified: false);
        db.Accounts.Add(account);
        db.PasswordResetTokens.Add(new PasswordResetToken(Guid.NewGuid(), account.Id, "invito", DateTimeOffset.UtcNow.AddDays(7)));
        await db.SaveChangesAsync();
        var passwords = new PasswordService();

        await new SetPasswordService(db, passwords).SetPasswordAsync("invito", "Password123", CancellationToken.None);

        var saved = await db.Accounts.SingleAsync();
        Assert.Equal(EAccountStatus.Active, saved.Status);
        Assert.NotNull(saved.EmailVerifiedAt);
        Assert.True(passwords.Verify("Password123", saved.PasswordHash));
        Assert.NotNull((await db.PasswordResetTokens.SingleAsync()).ConsumedAt);
    }

    [Fact]
    public async Task SetPassword_Throws_WhenTokenIsExpired()
    {
        using var db = TestSupport.NewDb();
        var account = TestSupport.NewAccount(EAccountRole.Association, "croce@example.com", verified: false);
        db.Accounts.Add(account);
        db.PasswordResetTokens.Add(new PasswordResetToken(Guid.NewGuid(), account.Id, "invito", DateTimeOffset.UtcNow.AddMinutes(-1)));
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            new SetPasswordService(db, new PasswordService()).SetPasswordAsync("invito", "Password123", CancellationToken.None));
    }

    [Fact]
    public async Task SetPassword_Throws_WhenTokenWasAlreadyUsed()
    {
        using var db = TestSupport.NewDb();
        var account = TestSupport.NewAccount(EAccountRole.Association, "croce@example.com", verified: false);
        db.Accounts.Add(account);
        db.PasswordResetTokens.Add(new PasswordResetToken(Guid.NewGuid(), account.Id, "invito", DateTimeOffset.UtcNow.AddDays(7)));
        await db.SaveChangesAsync();
        var service = new SetPasswordService(db, new PasswordService());
        await service.SetPasswordAsync("invito", "Password123", CancellationToken.None);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            service.SetPasswordAsync("invito", "AltraPassword1", CancellationToken.None));
    }
}
