using GoCare.Data;
using GoCare.Errors;
using GoCare.Models.Enums;
using GoCare.Services.Admin;
using GoCare.Services.Auth;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GoCare.Tests.Services.Admin;

public class AdminResendVerificationServiceTests
{
    private static AdminResendVerificationService NewService(GoCareDbContext db, FakeEmailSender email) =>
        new(db, new ResendVerificationEmailService(
            db, new TokenService(Options.Create(new JwtOptions())), email,
            Options.Create(new FrontendOptions { VerifyEmailUrl = "http://app/verify_email" })));

    [Fact]
    public async Task Resend_SendsNewVerificationEmail_ToUnverifiedCaregiver()
    {
        using var db = TestSupport.NewDb();
        var account = TestSupport.NewAccount(EAccountRole.Person, "mario@example.com", verified: false);
        db.Accounts.Add(account);
        await db.SaveChangesAsync();
        var email = new FakeEmailSender();

        await NewService(db, email).ResendAsync(account.Id, CancellationToken.None);

        var token = await db.EmailVerificationTokens.SingleAsync();
        var sent = Assert.Single(email.Sent);
        Assert.Equal("mario@example.com", sent.To);
        Assert.Contains(token.Token, sent.Body);
    }

    [Fact]
    public async Task Resend_Throws_WhenAccountIsAlreadyVerified()
    {
        using var db = TestSupport.NewDb();
        var account = TestSupport.NewAccount(EAccountRole.Person, "mario@example.com");
        db.Accounts.Add(account);
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<ConflictException>(() =>
            NewService(db, new FakeEmailSender()).ResendAsync(account.Id, CancellationToken.None));
    }

    [Fact]
    public async Task Resend_Throws_WhenCaregiverDoesNotExist()
    {
        using var db = TestSupport.NewDb();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            NewService(db, new FakeEmailSender()).ResendAsync(Guid.NewGuid(), CancellationToken.None));
    }
}
