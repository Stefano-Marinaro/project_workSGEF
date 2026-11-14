using GoCare.Infrastructure;
using GoCare.Services.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GoCare.Tests.Services.Auth;

public class RegisterUserServiceTests
{
    [Fact]
    public async Task Registration_PersistsNamesBeforeFirstLogin_AndRequiresVerification()
    {
        using var db = TestSupport.NewDb();
        var email = new FakeEmailSender();
        var service = new RegisterUserService(db, new PasswordService(),
            new TokenService(Options.Create(new JwtOptions())), email,
            Options.Create(new FrontendOptions { VerifyEmailUrl = "http://localhost/verify_email" }));

        var id = await service.RegisterUserAsync(" Test@Example.com ", "Password123!", " Anna ", " Rossi ", CancellationToken.None);

        var person = await db.Persons.SingleAsync(p => p.Id == id);
        Assert.Equal("Anna", person.Name);
        Assert.Equal("Rossi", person.Surname);
        Assert.Equal("test@example.com", person.Email);
        Assert.False(person.IsProfileComplete);
        Assert.False((await db.Accounts.SingleAsync()).CanLogIn);
        Assert.Single(email.Sent);
        Assert.Single(await db.EmailVerificationTokens.ToListAsync());
    }
}
