using GoCare.Data;
using GoCare.Errors;
using GoCare.Models.Auth;
using GoCare.Models.Enums;
using GoCare.Services.Admin;
using GoCare.Services.Auth;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GoCare.Tests.Services.Admin;

public class AdminPasswordResetServiceTests
{
    private static AdminPasswordResetService NewService(GoCareDbContext db, FakeEmailSender email) =>
        new(db, new ForgotPasswordService(
            db, new TokenService(Options.Create(new JwtOptions())), email,
            Options.Create(new FrontendOptions { ResetPasswordUrl = "http://app/reset_password" })));

    [Fact]
    public async Task Reset_RevokesSessions_AndSendsResetEmail()
    {
        using var db = TestSupport.NewDb();
        var account = TestSupport.NewAccount(EAccountRole.Person, "mario@example.com");
        db.Accounts.Add(account);
        db.RefreshTokens.Add(new RefreshToken(
            Guid.NewGuid(), account.Id, "sessione", DateTimeOffset.UtcNow.AddDays(7), DateTimeOffset.UtcNow));
        await db.SaveChangesAsync();
        var email = new FakeEmailSender();

        await NewService(db, email).ResetAsync(account.Id, CancellationToken.None);

        Assert.NotNull((await db.RefreshTokens.SingleAsync()).RevokedAt);
        var token = await db.PasswordResetTokens.SingleAsync();
        var sent = Assert.Single(email.Sent);
        Assert.Equal("mario@example.com", sent.To);
        Assert.Contains(token.Token, sent.Body);
    }

    [Fact]
    public async Task Reset_Throws_WhenAccountIsDeleted()
    {
        using var db = TestSupport.NewDb();
        var account = TestSupport.NewAccount(EAccountRole.Person, "mario@example.com");
        account.MarkDeleted(DateTimeOffset.UtcNow);
        db.Accounts.Add(account);
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<ConflictException>(() =>
            NewService(db, new FakeEmailSender()).ResetAsync(account.Id, CancellationToken.None));
    }

    [Fact]
    public async Task Reset_Throws_ForAnAssociationAccount()
    {
        using var db = TestSupport.NewDb();
        var association = TestSupport.NewAccount(EAccountRole.Association, "croce@example.com");
        db.Accounts.Add(association);
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            NewService(db, new FakeEmailSender()).ResetAsync(association.Id, CancellationToken.None));
    }
}
