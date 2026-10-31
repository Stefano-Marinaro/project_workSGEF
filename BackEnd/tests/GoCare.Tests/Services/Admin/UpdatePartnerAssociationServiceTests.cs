using GoCare.Data;
using GoCare.Errors;
using GoCare.Models.Auth;
using GoCare.Models.Domain;
using GoCare.Models.Enums;
using GoCare.Services.Admin;
using GoCare.Services.Auth;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GoCare.Tests.Services.Admin;

public class UpdatePartnerAssociationServiceTests
{
    private static UpdatePartnerAssociationService NewService(GoCareDbContext db, FakeEmailSender email) =>
        new(db, new TokenService(Options.Create(new JwtOptions())), email,
            Options.Create(new FrontendOptions { ChangeEmailUrl = "http://app/conferma-cambio-email" }));

    private static async Task<Account> SeedAssociation(GoCareDbContext db, bool active = true)
    {
        var account = TestSupport.NewAccount(EAccountRole.Association, "croce@example.com", verified: active);
        var association = new Association(account.Id, account.Email);
        association.UpdateProfile("Croce Verde", TestSupport.SampleAddress(), ["075123456"], null);
        db.Accounts.Add(account);
        db.Associations.Add(association);
        db.AssociationCoveredComuni.Add(new AssociationCoveredComune(account.Id, "054039"));
        await db.SaveChangesAsync();
        return account;
    }

    private static Task<bool> Update(UpdatePartnerAssociationService service, Guid id, string email) =>
        service.UpdateAsync(id, email, "Croce Verde Perugia", TestSupport.SampleAddress(), ["075999999"],
            ["054001"], "Sempre", CancellationToken.None);

    [Fact]
    public async Task Update_SavesProfileAndComuni_WithoutTouchingEmail()
    {
        using var db = TestSupport.NewDb();
        var account = await SeedAssociation(db);
        var email = new FakeEmailSender();

        var confirmationSent = await Update(NewService(db, email), account.Id, "CROCE@example.com");

        Assert.False(confirmationSent);
        var association = await db.Associations.SingleAsync();
        Assert.Equal("Croce Verde Perugia", association.Name);
        Assert.Equal(["075999999"], association.Phones!);
        Assert.Equal("054001", (await db.AssociationCoveredComuni.SingleAsync()).ComuneCodiceIstat);
        Assert.Empty(email.Sent);
    }

    [Fact]
    public async Task Update_SendsConfirmation_WhenEmailChanges()
    {
        using var db = TestSupport.NewDb();
        var account = await SeedAssociation(db);
        var email = new FakeEmailSender();

        var confirmationSent = await Update(NewService(db, email), account.Id, "nuova@example.com");

        Assert.True(confirmationSent);
        Assert.Equal("croce@example.com", (await db.Accounts.SingleAsync()).Email); // cambia solo dopo la conferma
        var token = await db.EmailChangeTokens.SingleAsync();
        Assert.Equal("nuova@example.com", token.NewEmail);
        Assert.Equal("nuova@example.com", Assert.Single(email.Sent).To);
    }

    [Fact]
    public async Task Update_Throws_WhenNewEmailIsTaken()
    {
        using var db = TestSupport.NewDb();
        var account = await SeedAssociation(db);
        db.Accounts.Add(TestSupport.NewAccount(EAccountRole.Person, "presa@example.com"));
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<ConflictException>(() =>
            Update(NewService(db, new FakeEmailSender()), account.Id, "presa@example.com"));
        Assert.Equal("Croce Verde", (await db.Associations.SingleAsync()).Name); // niente modifiche a metà
    }

    [Fact]
    public async Task Update_Throws_WhenEmailChangesBeforeInviteIsAccepted()
    {
        using var db = TestSupport.NewDb();
        var account = await SeedAssociation(db, active: false);

        await Assert.ThrowsAsync<ConflictException>(() =>
            Update(NewService(db, new FakeEmailSender()), account.Id, "nuova@example.com"));
    }

    [Fact]
    public async Task Update_Throws_WhenAssociationDoesNotExist()
    {
        using var db = TestSupport.NewDb();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            Update(NewService(db, new FakeEmailSender()), Guid.NewGuid(), "x@example.com"));
    }
}
