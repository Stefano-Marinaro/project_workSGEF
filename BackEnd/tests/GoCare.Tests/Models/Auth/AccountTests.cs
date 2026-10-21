using GoCare.Models.Auth;
using GoCare.Models.Enums;

using Xunit;

namespace GoCare.Tests.Models.Auth;

public class AccountTests
{
    private static Account NewAccount() =>
        new(Guid.NewGuid(), "mario.rossi@example.it", "hash", EAccountRole.Person, DateTimeOffset.UtcNow);

    private static Account AccountInStatus(EAccountStatus status)
    {
        var account = NewAccount();
        var now = DateTimeOffset.UtcNow;

        switch (status)
        {
            case EAccountStatus.Unverified:
                break;
            case EAccountStatus.Active:
                account.Verify(now);
                break;
            case EAccountStatus.Suspended:
                account.Verify(now);
                account.Suspend(now);
                break;
            case EAccountStatus.Deleted:
                account.MarkDeleted(now);
                break;
        }

        return account;
    }

    [Fact]
    public void NewAccount_StartsUnverified_AndCannotLogIn()
    {
        var account = NewAccount();

        Assert.Equal(EAccountStatus.Unverified, account.Status);
        Assert.False(account.CanLogIn);
        Assert.Null(account.EmailVerifiedAt);
        Assert.Null(account.SuspendedAt);
        Assert.Null(account.DeletedAt);
    }

    [Theory]
    [InlineData(EAccountStatus.Unverified, false)]
    [InlineData(EAccountStatus.Active, true)]
    [InlineData(EAccountStatus.Suspended, false)]
    [InlineData(EAccountStatus.Deleted, false)]
    public void CanLogIn_IsTrueOnlyWhenActive(EAccountStatus status, bool expected)
    {
        var account = AccountInStatus(status);

        Assert.Equal(expected, account.CanLogIn);
    }

    [Fact]
    public void Verify_FromUnverified_BecomesActive()
    {
        var account = NewAccount();
        var now = DateTimeOffset.UtcNow;

        account.Verify(now);

        Assert.Equal(EAccountStatus.Active, account.Status);
        Assert.Equal(now, account.EmailVerifiedAt);
    }

    [Theory]
    [InlineData(EAccountStatus.Active)]
    [InlineData(EAccountStatus.Suspended)]
    [InlineData(EAccountStatus.Deleted)]
    public void Verify_Throws_WhenNotUnverified(EAccountStatus status)
    {
        var account = AccountInStatus(status);

        Assert.Throws<InvalidOperationException>(() => account.Verify(DateTimeOffset.UtcNow));
    }

    [Theory]
    [InlineData(EAccountStatus.Unverified)]
    [InlineData(EAccountStatus.Active)]
    [InlineData(EAccountStatus.Suspended)]
    public void ChangePassword_Succeeds_WhenNotDeleted(EAccountStatus status)
    {
        var account = AccountInStatus(status);

        account.ChangePassword("new-hash");

        Assert.Equal("new-hash", account.PasswordHash);
    }

    [Fact]
    public void ChangePassword_Throws_WhenDeleted()
    {
        var account = AccountInStatus(EAccountStatus.Deleted);

        Assert.Throws<InvalidOperationException>(() => account.ChangePassword("new-hash"));
    }

    [Theory]
    [InlineData(EAccountStatus.Unverified)]
    [InlineData(EAccountStatus.Active)]
    [InlineData(EAccountStatus.Suspended)]
    public void ChangeEmail_Succeeds_WhenNotDeleted(EAccountStatus status)
    {
        var account = AccountInStatus(status);
        var now = DateTimeOffset.UtcNow;

        account.ChangeEmail("nuova@example.it", now);

        Assert.Equal("nuova@example.it", account.Email);
        Assert.Equal(now, account.EmailVerifiedAt);
    }

    [Fact]
    public void ChangeEmail_Throws_WhenDeleted()
    {
        var account = AccountInStatus(EAccountStatus.Deleted);

        Assert.Throws<InvalidOperationException>(
            () => account.ChangeEmail("nuova@example.it", DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Suspend_FromActive_BecomesSuspended()
    {
        var account = AccountInStatus(EAccountStatus.Active);
        var now = DateTimeOffset.UtcNow;

        account.Suspend(now);

        Assert.Equal(EAccountStatus.Suspended, account.Status);
        Assert.Equal(now, account.SuspendedAt);
    }

    [Fact]
    public void Suspend_FromUnverified_AlsoSucceeds()
    {
        // La guardia di Suspend blocca solo Deleted/Suspended: un account mai verificato
        // puo' finire direttamente Suspended. Comportamento reale, non ovvio dal solo nome del metodo.
        var account = AccountInStatus(EAccountStatus.Unverified);

        account.Suspend(DateTimeOffset.UtcNow);

        Assert.Equal(EAccountStatus.Suspended, account.Status);
    }

    [Theory]
    [InlineData(EAccountStatus.Suspended)]
    [InlineData(EAccountStatus.Deleted)]
    public void Suspend_Throws_WhenAlreadySuspendedOrDeleted(EAccountStatus status)
    {
        var account = AccountInStatus(status);

        Assert.Throws<InvalidOperationException>(() => account.Suspend(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Reinstate_FromSuspended_BecomesActive()
    {
        var account = AccountInStatus(EAccountStatus.Suspended);

        account.Reinstate();

        Assert.Equal(EAccountStatus.Active, account.Status);
        Assert.Null(account.SuspendedAt);
    }

    [Theory]
    [InlineData(EAccountStatus.Unverified)]
    [InlineData(EAccountStatus.Active)]
    [InlineData(EAccountStatus.Deleted)]
    public void Reinstate_Throws_WhenNotSuspended(EAccountStatus status)
    {
        var account = AccountInStatus(status);

        Assert.Throws<InvalidOperationException>(() => account.Reinstate());
    }

    [Theory]
    [InlineData(EAccountStatus.Unverified)]
    [InlineData(EAccountStatus.Active)]
    [InlineData(EAccountStatus.Suspended)]
    public void MarkDeleted_Succeeds_FromAnyNonDeletedStatus(EAccountStatus status)
    {
        var account = AccountInStatus(status);
        var now = DateTimeOffset.UtcNow;

        account.MarkDeleted(now);

        Assert.Equal(EAccountStatus.Deleted, account.Status);
        Assert.Equal(now, account.DeletedAt);
    }

    [Fact]
    public void MarkDeleted_Throws_WhenAlreadyDeleted()
    {
        var account = AccountInStatus(EAccountStatus.Deleted);

        Assert.Throws<InvalidOperationException>(() => account.MarkDeleted(DateTimeOffset.UtcNow));
    }
}
