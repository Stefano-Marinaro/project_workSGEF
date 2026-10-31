using GoCare.Data;
using GoCare.Errors;
using GoCare.Models.Enums;
using GoCare.Services.Admin;
using GoCare.Services.Auth;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GoCare.Tests.Services.Admin;

public class CreatePartnerAssociationServiceTests
{
    private static CreatePartnerAssociationService NewService(GoCareDbContext db, FakeEmailSender email) =>
        new(db, new PasswordService(), new TokenService(Options.Create(new JwtOptions())), email,
            Options.Create(new FrontendOptions { SetPasswordUrl = "http://app/set_password" }));

    private static Task<Guid> Create(CreatePartnerAssociationService service, string email = "croce@example.com") =>
        service.CreateAsync(email, "Croce Verde", TestSupport.SampleAddress(), ["075123456"], ["054039"], null,
            CancellationToken.None);

    [Fact]
    public async Task Create_SavesAccountAndProfile_AndSendsInvite()
    {
        using var db = TestSupport.NewDb();
        var email = new FakeEmailSender();

        var id = await Create(NewService(db, email));

        var account = await db.Accounts.SingleAsync(a => a.Id == id);
        Assert.Equal(EAccountRole.Association, account.Role);
        Assert.Equal(EAccountStatus.Unverified, account.Status); // invito in attesa: niente login

        var association = await db.Associations.SingleAsync(a => a.Id == id);
        Assert.Equal("Croce Verde", association.Name);
        Assert.Equal(EAccreditationStatus.Accredited, association.Status);
        Assert.Equal("054039", (await db.AssociationCoveredComuni.SingleAsync()).ComuneCodiceIstat);

        var token = await db.PasswordResetTokens.SingleAsync();
        Assert.Equal(id, token.AccountId);
        Assert.True(token.ExpiresAt > DateTimeOffset.UtcNow.AddDays(6));

        var sent = Assert.Single(email.Sent);
        Assert.Equal("croce@example.com", sent.To);
        Assert.Contains($"http://app/set_password?token={token.Token}", sent.Body);
    }

    [Fact]
    public async Task Create_Throws_WhenEmailIsAlreadyRegistered()
    {
        using var db = TestSupport.NewDb();
        db.Accounts.Add(TestSupport.NewAccount(EAccountRole.Person, "croce@example.com"));
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<ConflictException>(() => Create(NewService(db, new FakeEmailSender())));
    }
}
